// FILE: ground_and_go/Services/DailyProgressService.cs
using ground_and_go.Models;
using ground_and_go.Pages.WorkoutGeneration;
using System.Text.Json;

namespace ground_and_go.Services
{
    public class DailyProgressState
    {
        public int Step { get; set; }
        public double Progress { get; set; }
        public WorkoutLog? TodaysLog { get; set; }
    }

    public class DailyProgressService
    {
        private readonly Database _database;
        private readonly MockAuthService _authService;

        // --- HYBRID STATE (Preferences) ---
        
        public string CurrentFlowType
        {
            get => Preferences.Get(nameof(CurrentFlowType), "workout"); 
            set => Preferences.Set(nameof(CurrentFlowType), value ?? "workout");
        }

        public string? CurrentLogId
        {
            get => Preferences.Get(nameof(CurrentLogId), null);
            set
            {
                if (value == null) Preferences.Remove(nameof(CurrentLogId));
                else Preferences.Set(nameof(CurrentLogId), value);
            }
        }

        public FeelingResult? CurrentFeelingResult
        {
            get
            {
                string json = Preferences.Get(nameof(CurrentFeelingResult), string.Empty);
                return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<FeelingResult>(json);
            }
            set
            {
                if (value == null) Preferences.Remove(nameof(CurrentFeelingResult));
                else Preferences.Set(nameof(CurrentFeelingResult), JsonSerializer.Serialize(value));
            }
        }

        public EquipmentResult? CurrentEquipmentResult { get; set; }
        public Models.Workout? CurrentWorkout { get; set; } 

        public DailyProgressService(Database database, MockAuthService authService)
        {
            _database = database;
            _authService = authService;
        }

        public void ResetDailyState()
        {
            Preferences.Remove(nameof(CurrentFlowType));
            Preferences.Remove(nameof(CurrentLogId));
            Preferences.Remove(nameof(CurrentFeelingResult));
            CurrentWorkout = null;
            CurrentEquipmentResult = null;
        }

        // --- CORE LOGIC ---

        public async Task<DailyProgressState> GetTodaysProgressAsync()
        {
            string? memberId = _database.GetAuthenticatedMemberId();
            if (string.IsNullOrEmpty(memberId)) return new DailyProgressState { Step = 0, Progress = 0.0 };
            
            WorkoutLog? log = await _database.GetTodaysWorkoutLog(memberId);

            // STEP 0: NOT STARTED
            if (log == null)
            {
                ResetDailyState(); 
                return new DailyProgressState { Step = 0, Progress = 0.0 };
            }

            // RECOVERY LOGIC: If App Restarted, Local State (Preferences) might be stale or empty.
            // We need to re-populate CurrentFeelingResult so 'Resume' knows where to go.
            if (CurrentFeelingResult == null)
            {
                await RecoverStateFromLog(log);
            }

            // Ensure Log ID is tracked
            if (CurrentLogId == null) CurrentLogId = log.LogId;

            // CHECK: Is this a "Temporary State" log? (User is on Step 2)
            bool isTempState = log.BeforeJournal != null && log.BeforeJournal.StartsWith("STATE:");

            // STEP 1 COMPLETE -> USER IS ON STEP 2 (Journal Page)
            if (isTempState)
            {
                // We return Step 2.
                double progress = await GetProgressPercentageAsync(2); 
                return new DailyProgressState { Step = 2, Progress = progress, TodaysLog = log };
            }

            // STEP 2 COMPLETE -> USER IS ON STEP 3 (Mindfulness/Workout)
            // BeforeJournal is real text (not null, not STATE:) AND WorkoutId is not set yet
            if (!string.IsNullOrEmpty(log.BeforeJournal) && !isTempState && (log.WorkoutId == null || log.WorkoutId <= 0))
            {
                // We are at "Step 3" (Mindfulness or Workout Selection)
                double progress = await GetProgressPercentageAsync(3); 
                return new DailyProgressState { Step = 3, Progress = progress, TodaysLog = log };
            }

            // STEP 3/4 COMPLETE -> USER IS ON WORKOUT OR POST-JOURNAL
            if (log.WorkoutId > 0 && string.IsNullOrEmpty(log.AfterJournal))
            {
                // We are at "Step 4" (Workout In Progress)
                double progress = await GetProgressPercentageAsync(4);
                return new DailyProgressState { Step = 4, Progress = progress, TodaysLog = log };
            }

            // STEP 5 COMPLETE -> DONE
            if (!string.IsNullOrEmpty(log.AfterJournal))
            {
                return new DailyProgressState { Step = 5, Progress = 1.0, TodaysLog = log };
            }

            return new DailyProgressState { Step = 0, Progress = 0.0, TodaysLog = log };
        }

        private async Task RecoverStateFromLog(WorkoutLog log)
        {
            // STRATEGY 1: Extract from "STATE:{...}" string in BeforeJournal
            if (log.BeforeJournal != null && log.BeforeJournal.StartsWith("STATE:"))
            {
                try 
                {
                    string json = log.BeforeJournal.Substring(6); // Remove "STATE:"
                    var stateData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    
                    if (stateData != null)
                    {
                        if (stateData.ContainsKey("Flow")) CurrentFlowType = stateData["Flow"];
                        if (stateData.ContainsKey("Mood")) 
                        {
                            CurrentFeelingResult = new FeelingResult { Mood = stateData["Mood"], Rating = 5 };
                        }
                    }
                }
                catch { /* Ignore parse errors */ }
            }
            // STRATEGY 2: Infer from Workout ID (DB Relationship)
            else if (log.WorkoutId.HasValue && log.WorkoutId > 0)
            {
                string? emotion = await _database.GetEmotionNameByWorkoutId(log.WorkoutId.Value);
                
                if (!string.IsNullOrEmpty(emotion))
                {
                    CurrentFeelingResult = new FeelingResult { Mood = emotion, Rating = 5 };
                    CurrentFlowType = "workout"; // Assume workout if ID exists
                    
                    // Also reload the workout object
                    CurrentWorkout = await _database.GetWorkoutById(log.WorkoutId.Value);
                }
            }
            // STRATEGY 3: Fallback if we have real journal text but no workout ID yet (Rare edge case)
            // We assume "Rest" flow if we can't prove otherwise, or generic workout flow
            else if (!string.IsNullOrEmpty(log.BeforeJournal))
            {
                // Best guess default so Resume doesn't crash
                if (CurrentFeelingResult == null) 
                    CurrentFeelingResult = new FeelingResult { Mood = "Neutral", Rating = 5 };
            }
        }

        public async Task<bool> RequiresMindfulnessAsync()
        {
            if (CurrentFeelingResult?.Mood == null) return true;
            
            string mood = CurrentFeelingResult.Mood;

            // 1. Rest Day Logic
            if (CurrentFlowType == "rest")
            {
                 return await _database.HasMindfulnessActivitiesForEmotion(mood);
            }

            // 2. Workout Day Logic (Skip for Happy/Energized)
            var positiveMoods = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Happy", "Energized" };
            return !positiveMoods.Contains(mood);
        }

        public async Task<(int displayStep, int totalSteps)> GetDisplayStepAsync(int actualStep)
        {
            bool requiresMindfulness = await RequiresMindfulnessAsync();
            
            // Total Steps Calculation
            int totalSteps;
            if (CurrentFlowType == "rest") totalSteps = requiresMindfulness ? 4 : 3;
            else totalSteps = requiresMindfulness ? 5 : 4; 

            if (requiresMindfulness)
            {
                // REST: Map 5 -> 4.
                if (CurrentFlowType == "rest" && actualStep == 5) return (4, totalSteps);
                return (actualStep, totalSteps);
            }
            else
            {
                // NO MINDFULNESS (Skip Step 3)
                if (actualStep <= 2) return (actualStep, totalSteps);
                
                // REST (No Work, No Mind): Map 5 -> 3.
                if (CurrentFlowType == "rest")
                {
                    if (actualStep == 5) return (3, totalSteps);
                }
                
                // WORKOUT: Skip index 3
                if (actualStep >= 4) return (actualStep - 1, totalSteps);
            }

            return (actualStep, totalSteps);
        }

        public async Task<double> GetProgressPercentageAsync(int actualStep)
        {
            var (displayStep, totalSteps) = await GetDisplayStepAsync(actualStep);
            if (totalSteps == 0) return 0.0;
            return (double)(displayStep - 1) / totalSteps;
        }
    }
}
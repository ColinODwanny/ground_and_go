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

        // --- HYBRID STATE ---
        public string CurrentFlowType
        {
            get => Preferences.Get(nameof(CurrentFlowType), "workout"); 
            set => Preferences.Set(nameof(CurrentFlowType), value ?? "workout");
        }

        public string? CurrentLogId
        {
            get => Preferences.Get(nameof(CurrentLogId), null);
            set { if (value == null) Preferences.Remove(nameof(CurrentLogId)); else Preferences.Set(nameof(CurrentLogId), value); }
        }

        public FeelingResult? CurrentFeelingResult
        {
            get { string json = Preferences.Get(nameof(CurrentFeelingResult), string.Empty); return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<FeelingResult>(json); }
            set { if (value == null) Preferences.Remove(nameof(CurrentFeelingResult)); else Preferences.Set(nameof(CurrentFeelingResult), JsonSerializer.Serialize(value)); }
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

            if (log == null) { ResetDailyState(); return new DailyProgressState { Step = 0, Progress = 0.0 }; }

            // RECOVERY
            if (CurrentFeelingResult == null) await AttemptStateRecovery(log);
            if (CurrentLogId == null) CurrentLogId = log.LogId;

            bool isBeforeTemp = log.BeforeJournal != null && log.BeforeJournal.StartsWith("STATE:");
            bool isAfterTemp = log.AfterJournal != null && log.AfterJournal.StartsWith("STATE:");

            // STEP 2: JOURNAL PAGE (Resume)
            if (isBeforeTemp)
            {
                double progress = await GetProgressPercentageAsync(2); 
                return new DailyProgressState { Step = 2, Progress = progress, TodaysLog = log };
            }

            // STEP 3: MINDFULNESS / SELECTION (Resume)
            // Condition: Before journal done, NO temp flags, NO workout ID yet (Rest Day / Start of Workout flow).
            // FIX: Added '&& string.IsNullOrEmpty(log.AfterJournal)' 
            // This prevents a completed Rest Day (which has no workout ID) from being caught here.
            if (!string.IsNullOrEmpty(log.BeforeJournal) && 
                !isBeforeTemp && 
                !isAfterTemp && 
                string.IsNullOrEmpty(log.AfterJournal) &&
                (log.WorkoutId == null || log.WorkoutId <= 0))
            {
                double progress = await GetProgressPercentageAsync(3); 
                return new DailyProgressState { Step = 3, Progress = progress, TodaysLog = log };
            }

            // STEP 5 (Mapped): POST-JOURNAL PAGE (Resume)
            if (isAfterTemp)
            {
                // We are at Step 5 (Final Reflection)
                return new DailyProgressState { Step = 5, Progress = 0.95, TodaysLog = log };
            }

            // STEP 4: WORKOUT IN PROGRESS
            if (log.WorkoutId > 0 && string.IsNullOrEmpty(log.AfterJournal))
            {
                double progress = await GetProgressPercentageAsync(4);
                return new DailyProgressState { Step = 4, Progress = progress, TodaysLog = log };
            }

            // STEP 6: ALL COMPLETE
            // If we have AfterJournal text and it's NOT the temp state, we are done.
            if (!string.IsNullOrEmpty(log.AfterJournal) && !isAfterTemp)
            {
                return new DailyProgressState { Step = 6, Progress = 1.0, TodaysLog = log };
            }

            return new DailyProgressState { Step = 0, Progress = 0.0, TodaysLog = log };
        }

        private async Task AttemptStateRecovery(WorkoutLog log)
        {
            if (log.BeforeJournal != null && log.BeforeJournal.StartsWith("STATE:"))
            {
                try {
                    string json = log.BeforeJournal.Substring(6);
                    var stateData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    if (stateData != null) {
                        if (stateData.ContainsKey("Flow")) CurrentFlowType = stateData["Flow"];
                        if (stateData.ContainsKey("Mood")) CurrentFeelingResult = new FeelingResult { Mood = stateData["Mood"], Rating = 5 };
                    }
                } catch { }
            }
            else if (log.WorkoutId.HasValue && log.WorkoutId > 0)
            {
                string? emotion = await _database.GetEmotionNameByWorkoutId(log.WorkoutId.Value);
                if (!string.IsNullOrEmpty(emotion)) {
                    CurrentFeelingResult = new FeelingResult { Mood = emotion, Rating = 5 };
                    CurrentFlowType = "workout"; 
                    CurrentWorkout = await _database.GetWorkoutById(log.WorkoutId.Value);
                }
            }
            else if (!string.IsNullOrEmpty(log.BeforeJournal))
            {
                 // Fallback: If no workout ID, likely a Rest flow.
                 if (CurrentFlowType == "workout" && log.WorkoutId == null) CurrentFlowType = "rest";
                 if (CurrentFeelingResult == null) CurrentFeelingResult = new FeelingResult { Mood = "Neutral", Rating = 5 };
            }
        }

        public async Task<bool> RequiresMindfulnessAsync()
        {
            if (CurrentFeelingResult?.Mood == null) return true;
            string mood = CurrentFeelingResult.Mood;

            // Positive emotions skip mindfulness in ALL flows
            var positiveMoods = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Happy", "Energized" };
            if (positiveMoods.Contains(mood)) return false;

            // For Rest flow, double check DB for activities
            if (CurrentFlowType == "rest")
                 return await _database.HasMindfulnessActivitiesForEmotion(mood);

            return true; 
        }

        public async Task<int> GetTotalStepsAsync()
        {
            bool requiresMindfulness = await RequiresMindfulnessAsync();
            
            // Dynamic steps for Rest Flow
            if (CurrentFlowType == "rest")
            {
                return requiresMindfulness ? 4 : 3;
            }
            
            // Workout Flow
            return requiresMindfulness ? 5 : 4;
        }

        public async Task<(int displayStep, int totalSteps)> GetDisplayStepAsync(int actualStep)
        {
            bool requiresMindfulness = await RequiresMindfulnessAsync();
            int totalSteps = await GetTotalStepsAsync();

            // Map Step 6 (Complete) to 100%
            if (actualStep >= 6) return (totalSteps, totalSteps);

            // Step 5 (Post Journal) is always the last step
            if (actualStep == 5) return (totalSteps, totalSteps);

            if (requiresMindfulness)
            {
                // REST with Mindfulness: 1,2,3,5 -> Map 5 to 4.
                if (CurrentFlowType == "rest" && actualStep == 5) return (4, totalSteps);
                
                return (actualStep, totalSteps);
            }
            else
            {
                // NO MINDFULNESS (Skip Step 3)
                if (actualStep <= 2) return (actualStep, totalSteps);
                
                // REST (Short): 1, 2, 5. Map 5 -> 3.
                if (CurrentFlowType == "rest" && actualStep >= 5) return (3, totalSteps);
                
                // WORKOUT (Short): 1, 2, 4, 5. Map 4->3, 5->4.
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
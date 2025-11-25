using ground_and_go.Models;
using ground_and_go.Pages.WorkoutGeneration;

namespace ground_and_go.Services
{
    // This is the helper class. It just holds the result.
    // Any page can ask the service "What's the progress?" and
    // it will get one of these objects back.
    public class DailyProgressState
    {
        public int Step { get; set; }
        public double Progress { get; set; }
        public WorkoutLog? TodaysLog { get; set; } // pass the log along for convenience
    }

    // This is the main "brain" service
    public class DailyProgressService
    {
        private readonly Database _database;
        private readonly MockAuthService _authService;

        // This property will store our flow state
        public string? CurrentFlowType { get; set; }

        // This new property will store the ID of the log we just created
        public string? CurrentLogId { get; set; }

        // Store user's workout preferences for workout generation
        public FeelingResult? CurrentFeelingResult { get; set; }
        public EquipmentResult? CurrentEquipmentResult { get; set; }
        
        // Store the currently selected workout (especially for fallback workouts)
        public Models.Workout? CurrentWorkout { get; set; }

        // The service "requests" the database and mock auth service
        // and .NET MAUI gives them to us (this is dependency injection).
        public DailyProgressService(Database database, MockAuthService authService)
        {
            _database = database;
            _authService = authService;
        }

        // This is the main public function our pages will call
        public async Task<DailyProgressState> GetTodaysProgressAsync()
        {
            Console.WriteLine("DEBUG: DailyProgressService.GetTodaysProgressAsync() called");
            
            // 1. Get the (fake) logged-in user
            string? memberId = _database.GetAuthenticatedMemberId();
            Console.WriteLine($"DEBUG: Member ID: '{memberId ?? "NULL"}'");

            // 2. Ask the database for this user's log for today
            
            // if memberId is null, user isn't logged in, so no log.
            if (string.IsNullOrEmpty(memberId))
            {
                Console.WriteLine("DEBUG: Member ID is null, returning Step 0");
                 return new DailyProgressState { Step = 0, Progress = 0.0, TodaysLog = null };
            }
            
            WorkoutLog? log = await _database.GetTodaysWorkoutLog(memberId);
            Console.WriteLine($"DEBUG: Today's workout log found: {log != null}");

            // 3. Figure out the progress step based on the log
            
            // STEP 0: NOT STARTED
            // No log exists for this user today.
            if (log == null)
            {
                Console.WriteLine("DEBUG: No log found for today, returning Step 0");
                return new DailyProgressState { Step = 0, Progress = 0.0, TodaysLog = null };
            }

            Console.WriteLine($"DEBUG: Log details - LogId: '{log.LogId}', WorkoutId: {log.WorkoutId}, BeforeJournal: {!string.IsNullOrEmpty(log.BeforeJournal)}, AfterJournal: {!string.IsNullOrEmpty(log.AfterJournal)}");
            Console.WriteLine($"DEBUG: AfterJournal content: '{log.AfterJournal?.Substring(0, Math.Min(50, log.AfterJournal?.Length ?? 0))}...'");

            // STEP 1: PRE-JOURNAL COMPLETE
            // A log row exists, 'before_journal' is filled, but 'workout_id' is null (or -1, based on your model)
            if (!string.IsNullOrEmpty(log.BeforeJournal) && log.WorkoutId <= 0) // Using <= 0 to be safe
            {
                Console.WriteLine("DEBUG: Step 1 - Pre-journal complete, no workout yet");
                // We've found an existing log, so let's save its ID just in case
                CurrentLogId = log.LogId;
                return new DailyProgressState { Step = 1, Progress = 0.33, TodaysLog = log };
            }

            // STEP 2: WORKOUT GENERATED
            // A 'workout_id' has been saved, but the 'after_journal' is still empty.
            if (log.WorkoutId > 0 && string.IsNullOrEmpty(log.AfterJournal))
            {
                Console.WriteLine("DEBUG: Step 2 - Workout generated, awaiting completion");
                // We've found an existing log, so let's save its ID
                CurrentLogId = log.LogId;
                return new DailyProgressState { Step = 2, Progress = 0.66, TodaysLog = log };
            }

            // STEP 3: ALL COMPLETE
            // The 'after_journal' is filled. The user is done for the day.
            if (!string.IsNullOrEmpty(log.AfterJournal))
            {
                Console.WriteLine("DEBUG: Step 3 - All complete! After journal is filled");
                // We're all done, so we can clear the ID
                CurrentLogId = null;
                return new DailyProgressState { Step = 3, Progress = 1.0, TodaysLog = log };
            }

            Console.WriteLine("DEBUG: Unexpected state - falling back to Step 0");
            Console.WriteLine($"DEBUG: Unexpected state details - BeforeJournal empty: {string.IsNullOrEmpty(log.BeforeJournal)}, WorkoutId: {log.WorkoutId}, AfterJournal empty: {string.IsNullOrEmpty(log.AfterJournal)}");
            // Failsafe, in case of a weird state we didn't predict
            return new DailyProgressState { Step = 0, Progress = 0.0, TodaysLog = log };
        }

        /// <summary>
        /// Determines if the current emotion requires mindfulness by checking database
        /// Uses workout-specific logic that excludes Happy/Energized from mindfulness
        /// </summary>
        /// <returns>True if mindfulness activities exist for this emotion, false if skipped</returns>
        public async Task<bool> RequiresMindfulnessAsync()
        {
            if (CurrentFeelingResult?.Mood == null) return false;
            
            // Use workout-specific logic that excludes Happy/Energized
            return await _database.HasWorkoutMindfulnessActivitiesForEmotion(CurrentFeelingResult.Mood);
        }
        
        /// <summary>
        /// Determines if the current emotion requires mindfulness (synchronous version for backward compatibility)
        /// NOTE: This will be deprecated - use RequiresMindfulnessAsync instead
        /// </summary>
        /// <returns>True if mindfulness is required, false if skipped</returns>
        public bool IsNegativeEmotion()
        {
            // For now, keep the old logic as fallback, but this should be replaced with async calls
            if (CurrentFeelingResult?.Mood == null) return false;
            
            var negativeEmotions = new HashSet<string> { "Sad", "Depressed", "Tired", "Angry", "Anxious" };
            return negativeEmotions.Contains(CurrentFeelingResult.Mood);
        }

        /// <summary>
        /// Gets the total number of steps for the current flow (async version)
        /// </summary>
        /// <returns>4 for emotions without mindfulness, 5 for emotions with mindfulness</returns>
        public async Task<int> GetTotalStepsAsync()
        {
            bool requiresMindfulness = await RequiresMindfulnessAsync();
            return requiresMindfulness ? 5 : 4;
        }

        /// <summary>
        /// Gets the display step number and total for a given actual step (async version)
        /// </summary>
        /// <param name="actualStep">The actual step number (1=emotion, 2=journal, 3=mindfulness, 4=workout, 5=post-journal)</param>
        /// <returns>Tuple of (displayStep, totalSteps)</returns>
        public async Task<(int displayStep, int totalSteps)> GetDisplayStepAsync(int actualStep)
        {
            bool requiresMindfulness = await RequiresMindfulnessAsync();
            int totalSteps = requiresMindfulness ? 5 : 4;
            
            if (requiresMindfulness)
            {
                // Emotions with mindfulness: all steps are displayed as-is
                return (actualStep, totalSteps);
            }
            else
            {
                // Emotions without mindfulness: skip step 3 (mindfulness)
                // actualStep 1 -> display 1, actualStep 2 -> display 2, actualStep 4 -> display 3, actualStep 5 -> display 4
                int displayStep = actualStep <= 2 ? actualStep : actualStep - 1;
                return (displayStep, totalSteps);
            }
        }

        /// <summary>
        /// Gets the progress percentage for a given step (async version)
        /// </summary>
        /// <param name="actualStep">The actual step number</param>
        /// <returns>Progress percentage (0.0 to 1.0)</returns>
        public async Task<double> GetProgressPercentageAsync(int actualStep)
        {
            var (displayStep, totalSteps) = await GetDisplayStepAsync(actualStep);
            return (displayStep - 1) / (double)totalSteps;
        }

        /// <summary>
        /// Gets the total number of steps for the current flow (synchronous fallback)
        /// </summary>
        /// <returns>4 for good emotions, 5 for bad emotions</returns>
        public int GetTotalSteps()
        {
            return IsNegativeEmotion() ? 5 : 4;
        }

        /// <summary>
        /// Gets the display step number and total for a given actual step (synchronous fallback)
        /// </summary>
        /// <param name="actualStep">The actual step number (1=emotion, 2=journal, 3=mindfulness, 4=workout, 5=post-journal)</param>
        /// <returns>Tuple of (displayStep, totalSteps)</returns>
        public (int displayStep, int totalSteps) GetDisplayStep(int actualStep)
        {
            bool isNegative = IsNegativeEmotion();
            int totalSteps = isNegative ? 5 : 4;
            
            if (isNegative)
            {
                // Bad emotions: all steps are displayed as-is
                return (actualStep, totalSteps);
            }
            else
            {
                // Good emotions: skip step 3 (mindfulness)
                // actualStep 1 -> display 1, actualStep 2 -> display 2, actualStep 4 -> display 3, actualStep 5 -> display 4
                int displayStep = actualStep <= 2 ? actualStep : actualStep - 1;
                return (displayStep, totalSteps);
            }
        }

        /// <summary>
        /// Gets the progress percentage for a given step (synchronous fallback)
        /// </summary>
        /// <param name="actualStep">The actual step number</param>
        /// <returns>Progress percentage (0.0 to 1.0)</returns>
        public double GetProgressPercentage(int actualStep)
        {
            var (displayStep, totalSteps) = GetDisplayStep(actualStep);
            return (displayStep - 1) / (double)totalSteps;
        }
    }
}
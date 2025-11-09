using ground_and_go.Models; // We need this for the 'WorkoutLog' model

namespace ground_and_go.Services
{
    // This is the helper class. It just holds the result.
    // Any page can ask the service "What's the progress?" and
    // it will get one of these objects back.
    public class DailyProgressState
    {
        public int Step { get; set; }
        public double Progress { get; set; }
        public WorkoutLog? TodaysLog { get; set; } // We'll pass the log along for convenience
    }

    // This is the main "brain" service
    public class DailyProgressService
    {
        private readonly Database _database;
        private readonly MockAuthService _authService;

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
            // 1. Get the (fake) logged-in user
            int memberId = _authService.GetCurrentMemberId();

            // 2. Ask the database for this user's log for today
            //
            // !!! NOTE: This 'GetTodaysWorkoutLog' function doesn't exist yet!
            // We will create it in Database.cs in the next chunk.
            // For now, we're just writing the code that *will* use it.
            //
            WorkoutLog log = await _database.GetTodaysWorkoutLog(memberId);

            // 3. Figure out the progress step based on the log
            
            // STEP 0: NOT STARTED
            // No log exists for this user today.
            if (log == null)
            {
                return new DailyProgressState { Step = 0, Progress = 0.0, TodaysLog = null };
            }

            // STEP 1: PRE-JOURNAL COMPLETE
            // A log row exists, 'before_journal' is filled, but 'workout_id' is null (or -1, based on your model)
            if (!string.IsNullOrEmpty(log.BeforeJournal) && log.WorkoutId <= 0) // Using <= 0 to be safe
            {
                return new DailyProgressState { Step = 1, Progress = 0.33, TodaysLog = log };
            }

            // STEP 2: WORKOUT GENERATED
            // A 'workout_id' has been saved, but the 'after_journal' is still empty.
            if (log.WorkoutId > 0 && string.IsNullOrEmpty(log.AfterJournal))
            {
                return new DailyProgressState { Step = 2, Progress = 0.66, TodaysLog = log };
            }

            // STEP 3: ALL COMPLETE
            // The 'after_journal' is filled. The user is done for the day.
            if (!string.IsNullOrEmpty(log.AfterJournal))
            {
                return new DailyProgressState { Step = 3, Progress = 1.0, TodaysLog = log };
            }

            // Failsafe, in case of a weird state we didn't predict
            return new DailyProgressState { Step = 0, Progress = 0.0, TodaysLog = log };
        }
    }
}
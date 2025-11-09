using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using Supabase.Gotrue;
using System.Collections.ObjectModel;
using ground_and_go.Models;

namespace ground_and_go
{
    public class Database
    {
        private Supabase.Client? supabaseClient;
        private Task waitingForInitialization;

        public List<WorkoutLog>? WorkoutHistory { get; set; }
        public static Dictionary<int, Exercise>? ExercisesDictionary { get; set; }
        public Database()
        {
            waitingForInitialization = InitializeSupabaseSystems();
        }
        public Task EnsureInitializedAsync() => waitingForInitialization ?? Task.CompletedTask;

        private async Task InitializeSupabaseSystems()
        {
            var supabaseProjectURL = "https://irekjohmgsjicpszbgus.supabase.co";
            var supabaseProjectKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImlyZWtqb2htZ3NqaWNwc3piZ3VzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjEwODcwMTQsImV4cCI6MjA3NjY2MzAxNH0.vAMY-0u9u2hNbGIcg4h7tdhI6cOW5jUMcMWEP67ChxQ";

            supabaseClient = new Supabase.Client(supabaseProjectURL, supabaseProjectKey);
            await supabaseClient.InitializeAsync();
            Console.WriteLine("after supabase client init");

            await LoadExercises();
        }

        /// <summary>
        /// Queries the database to find a member's ID using their email address.
        /// </summary>
        /// <param name="userEmail">The email address of the member to look up</param>
        /// <returns>The member's ID if found, or -1 if no member exists with the given email</returns>
        /// <remarks>
        /// This method ensures the Supabase client is initialized before querying.
        /// </remarks>
        public async Task<int> GetMemberIdByEmail(string userEmail)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return -1;
            
            int memberId;
            var response = await supabaseClient!.From<Member>().Where(member => member.Email == userEmail).Get();

            if (response?.Models?.Any() == true) //If the query returned any rows
            {
                memberId = response.Models[0].MemberId;
            }
            else
            {
                memberId = -1;
            }
            return memberId;
        }


        /// <summary>
        /// Loads the workout history for a specific member into the WorkoutHistory property.
        /// </summary>
        /// <param name="memberId">The ID of the member whose workout history to load</param>
        /// <remarks>
        /// This method populates the WorkoutHistory property with the query results.
        /// If no workout history exists, WorkoutHistory remains unchanged.
        /// </remarks>
        public async Task LoadWorkoutHistory(int memberId)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return;
            
            var response = await supabaseClient.From<WorkoutLog>().Where(workoutLog => workoutLog.MemberId == memberId).Get();
            if (response?.Models?.Any() == true)
            {
                WorkoutHistory = response.Models;
            }
        }


        /// <summary>
        /// Loads all exercises from the database into the ExercisesDictionary property.
        /// </summary>
        /// <remarks>
        /// This method populates the ExercisesDictionary where the key is the ExerciseId
        /// and the value is the Exercise object. Ensure the Supabase client is initialized
        /// before calling this method.
        /// </remarks>
        /// THIS METHOD DOES NOT WORK CURRENTLY
        public async Task LoadExercises()
{
    await EnsureInitializedAsync();
    if (supabaseClient == null) return;

    ExercisesDictionary = new Dictionary<int, Exercise>();

    try
    {
        var response = await supabaseClient.From<Exercise>().Get();
        if (response?.Models != null)
        {
            foreach (var exercise in response.Models)
            {
                ExercisesDictionary[exercise.ExerciseId] = exercise;
            }
            Console.WriteLine($"Loaded {ExercisesDictionary.Count} exercises into dictionary.");
        }
        else
        {
            Console.WriteLine("No exercises found in database.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading exercises: {ex.Message}");
    }
}


        /// <summary>
        /// Gets workout details by workout ID, including exercise information
        /// </summary>
        /// <param name="workoutId">The ID of the workout to fetch</param>
        /// <returns>Workout object with exercise details</returns>
        public async Task<Workout?> GetWorkoutById(int workoutId)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return null;
            
            var response = await supabaseClient.From<Workout>().Where(w => w.WorkoutId == workoutId).Get();
            return response?.Models?.FirstOrDefault();
        }

        /// <summary>
        /// Gets all exercises to build a lookup dictionary
        /// </summary>
        /// <returns>Dictionary mapping exercise IDs to Exercise objects</returns>
        public async Task<Dictionary<int, Exercise>> GetAllExercises()
        {
            await EnsureInitializedAsync();
            var exercisesDict = new Dictionary<int, Exercise>();
            
            if (supabaseClient == null) return exercisesDict;
            
            try
            {
                var response = await supabaseClient.From<Exercise>().Get();
                if (response?.Models != null)
                {
                    foreach (var exercise in response.Models)
                    {
                        exercisesDict[exercise.ExerciseId] = exercise;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading exercises: {ex.Message}");
            }
            
            return exercisesDict;
        }

        /// <summary>
        /// Gets workout logs with complete details including exercises
        /// </summary>
        /// <param name="memberId">Member ID to filter by</param>
        /// <returns>List of WorkoutLogViewModel with complete information</returns>
        public async Task<List<WorkoutLogViewModel>> GetWorkoutLogsWithDetails(int? memberId = null)
        {
            await EnsureInitializedAsync();
            
            var result = new List<WorkoutLogViewModel>();
            if (supabaseClient == null) return result;
            
            try
            {
                // Get ALL workout logs - no filtering by member ID for now
                Console.WriteLine("Getting ALL workout logs from database");
                var workoutLogsResponse = await supabaseClient.From<WorkoutLog>().Get();
                
                // Commented out member ID filtering logic for now
                // var query = supabaseClient.From<WorkoutLog>();
                // if (memberId.HasValue)
                // {
                //     Console.WriteLine($"Filtering workout logs by member ID: {memberId.Value}");
                //     query = query.Where(log => log.MemberId == memberId.Value);
                // }
                // else
                // {
                //     Console.WriteLine("Getting ALL workout logs from database");
                // }
                // var workoutLogsResponse = await query.Get();
                Console.WriteLine($"Retrieved {workoutLogsResponse?.Models?.Count ?? 0} workout logs from database");
                
                if (workoutLogsResponse?.Models != null)
                {
                    foreach (var log in workoutLogsResponse.Models)
                    {
                        var viewModel = new WorkoutLogViewModel(log);
                        
                        // Get workout details only (excluding problematic info column)
                        try
                        {
                            var workoutResponse = await supabaseClient.From<Workout>()
                                .Select("workout_id, emotion_id, category, category_num, equipment, impact, exercises")
                                .Where(w => w.WorkoutId == log.WorkoutId)
                                .Get();
                            
                            viewModel.WorkoutDetails = workoutResponse?.Models?.FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading workout details for log {log.LogId}: {ex.Message}");
                        }
                        
                        result.Add(viewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetWorkoutLogsWithDetails: {ex.Message}");
            }
            
            return result.OrderByDescending(x => x.WorkoutLog.DateTime).ToList();
        }

        /// <summary>
        /// Gets all workouts from the workouts table with dates from workout_log
        /// </summary>
        /// <returns>List of WorkoutViewModel with dates</returns>
        public async Task<List<WorkoutViewModel>> GetAllWorkoutsWithDates()
        {
            await EnsureInitializedAsync();
            
            var result = new List<WorkoutViewModel>();
            if (supabaseClient == null) 
            {
                Console.WriteLine("Supabase client is null");
                return result;
            }
            
            try
            {
                Console.WriteLine("Querying workouts table...");
                var workoutsResponse = await supabaseClient.From<Workout>()
                    .Select("workout_id, emotion_id, category, category_num, equipment, impact, exercises")
                    .Get();
                Console.WriteLine($"Workouts response received. Models count: {workoutsResponse?.Models?.Count ?? 0}");
                
                Console.WriteLine("Querying workout_log table for dates...");
                var workoutLogsResponse = await supabaseClient.From<WorkoutLog>()
                    .Select("workout_id, date")
                    .Get();
                Console.WriteLine($"Workout logs response received. Models count: {workoutLogsResponse?.Models?.Count ?? 0}");
                
                if (workoutsResponse?.Models != null)
                {
                    var workoutLogDates = workoutLogsResponse?.Models?
                        .GroupBy(log => log.WorkoutId)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(log => log.DateTime).First().DateTime) ?? new Dictionary<int, DateTime>();
                    
                    foreach (var workout in workoutsResponse.Models.OrderBy(w => w.WorkoutId))
                    {
                        var viewModel = new WorkoutViewModel(workout);
                        
                        // Set the date from workout_log if available
                        if (workoutLogDates.ContainsKey(workout.WorkoutId))
                        {
                            viewModel.WorkoutDate = workoutLogDates[workout.WorkoutId];
                        }
                        
                        result.Add(viewModel);
                        Console.WriteLine($"Added workout: ID={workout.WorkoutId}, Category={workout.Category}, Date={viewModel.DisplayDate}, Exercises=[{string.Join(",", workout.Exercises ?? new int[0])}]");
                    }
                }
                else
                {
                    Console.WriteLine("Workouts response or Models is null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading workouts: {ex.Message}");
            }
            
            return result;
        }

        /// <summary>
        /// Test method to debug data structure and content
        /// </summary>
        public async Task DebugDatabaseContent()
        {
            await EnsureInitializedAsync();
            
            if (supabaseClient == null) return;
            
            Console.WriteLine("=== DEBUGGING DATABASE CONTENT ===");
            
            // Test workout_log table
            var workoutLogs = await supabaseClient.From<WorkoutLog>().Limit(5).Get();
            Console.WriteLine($"\n--- WORKOUT LOGS (first 5) ---");
            if (workoutLogs?.Models != null)
            {
                foreach (var log in workoutLogs.Models)
                {
                    Console.WriteLine($"LogId: {log.LogId}");
                    Console.WriteLine($"MemberId: {log.MemberId}");
                    Console.WriteLine($"WorkoutId: {log.WorkoutId}");
                    Console.WriteLine($"Date: {log.DateTime}");
                    Console.WriteLine($"BeforeJournal: {log.BeforeJournal?.Substring(0, Math.Min(50, log.BeforeJournal?.Length ?? 0))}...");
                    Console.WriteLine($"AfterJournal: {log.AfterJournal?.Substring(0, Math.Min(50, log.AfterJournal?.Length ?? 0))}...");
                    Console.WriteLine("---");
                }
            }

            // Skip workouts table for now due to JSON parsing issues
            Console.WriteLine($"\n--- WORKOUTS (skipped due to JSON parsing issues) ---");

            // Test exercises table  
            Console.WriteLine($"\n--- EXERCISES (ALL) ---");
            try
            {
                var exercises = await supabaseClient.From<Exercise>().Get();
                Console.WriteLine($"Found {exercises?.Models?.Count ?? 0} exercises in database");
                if (exercises?.Models != null)
                {
                    foreach (var exercise in exercises.Models)
                    {
                        Console.WriteLine($"ExerciseId: {exercise.ExerciseId}");
                        Console.WriteLine($"Name: '{exercise.Name}'");
                        Console.WriteLine($"VideoLink: '{exercise.VideoLink}'");
                        Console.WriteLine("---");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading exercises: {ex.Message}");
            }
        }

        /// <summary>
        /// Takes the inputted journal entry and stores it in the database, leaving the after_journal null until updated
        /// </summary>
        /// <param name="entry">The inputted journal log to store</param>
        /// <returns>A task</returns>
        public async Task UploadJournalEntry(String entry, int workoutId)
        {
            await EnsureInitializedAsync();
            try
            {
                //TODO Get user UUID once implemented
                int memberId = 1; //Placeholder id

                WorkoutLog logEntry = new WorkoutLog();
                logEntry.WorkoutId = workoutId;
                logEntry.MemberId = memberId;
                logEntry.BeforeJournal = entry;
                logEntry.DateTime = DateTime.Now;
                //logEntry.AfterJournal will be filled in after the workout, so it will be null for now

                var response = await supabaseClient!.From<WorkoutLog>().Insert(logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ATTN: Error while inserting journal entry -- {ex.ToString()}");
            }
        }

        // This function gets the workout log for a specific member on today's date
        public async Task<WorkoutLog?> GetTodaysWorkoutLog(int memberId)
        {
            // Make sure the client is ready
            await EnsureInitializedAsync();
            if (supabaseClient == null)
            {
                Console.WriteLine("Supabase client is not initialized.");
                return null;
            }

            // Get today's date. We use .Date to make sure we're only comparing
            // the day, not the time.
            var today = DateTime.Today;

            try
            {
                // Query the 'workout_log' table using your 'supabaseClient'
                var response = await supabaseClient.From<WorkoutLog>()
                    .Where(log => log.MemberId == memberId && log.DateTime.Date == today)
                    .Limit(1) // We only expect one log per day
                    .Get();

                // Return the first log it finds, or null if none exist
                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error fetching today's workout log: {ex.Message}");
                return null; // Return null if anything goes wrong
            }
        }

        // This function creates the *initial* log entry for the day
        // It only saves the memberId, date, and before_journal
        public async Task<WorkoutLog?> CreateInitialWorkoutLog(int memberId, string beforeJournalText)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return null;

            try
            {
                var newLog = new WorkoutLog
                {
                    MemberId = memberId,
                    BeforeJournal = beforeJournalText,
                    // *** BUG 2 FIX: Use local time (Now) to match the query (Today) ***
                    DateTime = DateTime.Now // Was UtcNow
                };

                var response = await supabaseClient.From<WorkoutLog>()
                                                    .Insert(newLog);

                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating initial workout log: {ex.Message}");
                return null;
            }
        }
        
        // This function updates an existing log with the 'after_journal' text
        public async Task UpdateAfterJournalAsync(int logId, string afterJournalText)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return;

            try
            {
                // We find the log by its 'log_id' and update only the 'after_journal' column
                await supabaseClient.From<WorkoutLog>()
                                    .Where(log => log.LogId == logId)
                                    .Set(log => log.AfterJournal, afterJournalText)
                                    .Update();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating after_journal: {ex.Message}");
            }
        }

        // *** BUILD ERROR FIX: Add the missing method ***
        // This function updates an existing log with the 'workout_id'
        public async Task UpdateWorkoutIdAsync(int logId, int workoutId)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return;

            try
            {
                // We find the log by its 'log_id' and update only the 'workout_id' column
                await supabaseClient.From<WorkoutLog>()
                                    .Where(log => log.LogId == logId)
                                    .Set(log => log.WorkoutId, workoutId)
                                    .Update();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating workout_id: {ex.Message}");
            }
        }
    }
}
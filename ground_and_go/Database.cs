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
using Microsoft.Maui.Graphics.Text;
using ground_and_go.enums;
using static Supabase.Postgrest.Constants;







namespace ground_and_go
{
    public class Database
    {
        private Supabase.Client? supabaseClient;
        private Task waitingForInitialization;

        public List<WorkoutLog>? WorkoutHistory { get; set; }
        public static Dictionary<int, Exercise>? ExercisesDictionary { get; set; }

        public static List<MindfulnessActivity>? MindfulnessActivities { get; set; }


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


        }

        /// <summary>
        /// Gets the currently logged-in user's UUID (as a string).
        /// </summary>
        /// <returns>The user's UUID string, or null if not logged in.</returns>
        public string? GetAuthenticatedMemberId()
        {
            return supabaseClient?.Auth?.CurrentUser?.Id;
        }

        /// <summary>
        /// Sets a Supabase session for when the user is redirected back to app
        /// </summary>
        /// <param name="accessToken">The accessToken passed from the email link</param>
        /// <param name="refreshToken">The refreshToken passed from the email link</param>
        public async void SetSupabaseSession(string accessToken, string refreshToken)
        {
            if (supabaseClient != null)
            {
                await supabaseClient.Auth.SetSession(accessToken, refreshToken, true);
            }
        }


        /// <summary>
        /// Communicates with Supabase to log the user in with the given credentials
        /// </summary>
        /// <param name="username">The email to log the user in with</param>
        /// <param name="password">The password to log the user in with</param>
        /// <returns>Null if successful, an error string otherwise</returns>
        public async Task<String?> LogIn(String username, String password)
        {
            try
            {
                // 1. Perform the Auth Login
                Session? session = await supabaseClient!.Auth.SignInWithPassword(username, password);

                if (session != null && session.User != null)
                {
                    // 2. SAFETY CHECK: Does this user have a Member profile?
                    // If they deleted their account, this row will be missing.
                    var memberCheck = await supabaseClient
                        .From<Member>()
                        .Where(x => x.MemberId == session.User.Id)
                        .Get();

                    // If no member row found, this is a "Ghost" account (deleted user)
                    if (memberCheck.Models.Count == 0)
                    {
                        Console.WriteLine("Login blocked: User authenticated but has no Member profile (Account Deleted).");

                        // Force logout immediately
                        await supabaseClient.Auth.SignOut();

                        return "This account has been deleted.";
                    }

                    Console.WriteLine("Login successful");
                    return null;
                }

                return "Login failed";
            }
            catch (Supabase.Gotrue.Exceptions.GotrueException e)
            {
                Console.WriteLine($"Login failed: {e}");
                return "Invalid login credentials";
            }
            catch (Exception e)
            {
                Console.WriteLine($"Login error: {e}");
                return "An error has occurred - Please try again later";
            }
        }

        /// <summary>
        /// Communicates with Supabase to create an accout for the user with the given credentials
        /// </summary>
        /// <param name="username">The email to sign the user up with</param>
        /// <param name="password">The password to sign the user up with</param>
        /// <returns>Null if successful, an error string otherwise</returns>
        public async Task<String?> SignUp(String username, String password)
        {
            try
            {
                await supabaseClient!.Auth.SignOut();
                var session = await supabaseClient.Auth.SignUp(username, password);

                Console.WriteLine("Signup was successful");
                return null; //The user has successfully signed up
            }
            catch (Exception e)
            {
                Console.WriteLine($"Signup failed: {e}");
                return "An error has occurred while signing up";
            }
        }

        /// <summary>
        /// Communicates with Supabase to log out the current user
        /// </summary>
        /// <returns>A task</returns>
        public async Task LogOut()
        {
            try
            {
                await supabaseClient!.Auth.SignOut();
                Console.WriteLine("Logout successful");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Logout error: {e.Message}");
            }
        }

        /// <summary>
        /// Sends an email to the user for resetting their password
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>Null if successful, the appropriate error string otherwise</returns>
        public async Task<string?> ForgotPassword(string email)
        {
            try
            {
                await supabaseClient!.Auth.ResetPasswordForEmail(email);
                Console.WriteLine("Password reset successful.");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Password reset error: {e.Message}");
                return "An error has occurred while resetting password.";
            }
        }

        /// <summary>
        /// Changes the user's password in Supabase
        /// </summary>
        /// <param name="password">The new password to replace the old one</param>
        /// <returns>Null if successful, the appropriate error string otherwise</returns>
        public async Task<string?> ChangePassword(string password)
        {
            try
            {
                var result = await supabaseClient!.Auth.Update(
                    new Supabase.Gotrue.UserAttributes
                    {
                        Password = password,
                    }
                );
                if (result == null) //Returned user is null - an error occurred
                {
                    return "User was not found.";
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Password change error: {e.Message}");
                return "An error has occurred while changing password.";
            }
        }

        /// <summary>
        /// Queries the database to find a member's ID using their email address.
        /// </summary>
        /// <param name="userEmail">The email address of the member to look up</param>
        /// <returns>The member's ID if found, or -1 if no member exists with the given email</returns>
        /// <remarks>
        /// This method ensures the Supabase client is initialized before querying.
        /// </remarks>
        public async Task<string> GetMemberIdByEmail(string userEmail)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return "-1";

            string memberId;
            var response = await supabaseClient!.From<Member>().Where(member => member.Email == userEmail).Get();

            if (response?.Models?.Any() == true) //If the query returned any rows
            {
                memberId = response.Models[0].MemberId;
            }
            else
            {
                memberId = "-1";
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

            // This query will fail if member_id is a UUID. 
            // We can fix it later if needed, but it's not blocking the journal insert.
            // var response = await supabaseClient.From<WorkoutLog>().Where(workoutLog => workoutLog.MemberId == memberId).Get();

            // For now, let's just log a warning
            Console.WriteLine("WARNING: LoadWorkoutHistory is using an 'int' memberId, but schema is 'uuid'. This method will not work.");
            // if (response?.Models?.Any() == true)
            // {
            //     WorkoutHistory = response.Models;
            // }
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
                    Console.WriteLine($"Loaded {ExercisesDictionary.Count} exercises into dictionary.q");
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

        public async Task LoadMindfulness()
        {
            Console.WriteLine("inside of LoadMindfulness()");
            await EnsureInitializedAsync();
            if (supabaseClient == null) return;
            Console.WriteLine("inside of if statemenet");

            MindfulnessActivities = new List<MindfulnessActivity>();

            try
            {
                var response = await supabaseClient.From<MindfulnessActivity>().Get();
                Console.WriteLine(response);
                if (response?.Models != null)
                {

                    foreach (var activity in response.Models)
                    {
                        MindfulnessActivities.Add(activity);
                    }
                    Console.WriteLine($"Loaded {MindfulnessActivities.Count} minfulness activities into list.");
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

        public async Task<MindfulnessActivity?> GetMindfulnessActivityByEmotion(Emotion emotion)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return null;

            int emotionValue = (int)emotion;

            var response = await supabaseClient
                .From<MindfulnessActivity>()
                .Filter(
                    "associated_emotions",
                    Operator.Contains,
                    new List<int> { emotionValue }
                )
                .Get();

            var results = response.Models;

            if (results == null || results.Count == 0)
                return null;

            // Pick random
            var rng = new Random();
            int index = rng.Next(results.Count);

            return results[index];
        }

        /// <summary>
        /// Checks if any mindfulness activities exist for the given emotion
        /// </summary>
        /// <param name="emotionString">The emotion string (e.g., "Happy", "Sad", etc.)</param>
        /// <returns>True if mindfulness activities exist, false otherwise</returns>
        public async Task<bool> HasMindfulnessActivitiesForEmotion(string emotionString)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return false;

            // Convert string emotion to enum, then to int
            if (!Enum.TryParse<Emotion>(emotionString.ToUpper(), out var emotion))
            {
                Console.WriteLine($"DEBUG: Could not parse emotion '{emotionString}' to enum");
                return false;
            }

            int emotionValue = (int)emotion;
            Console.WriteLine($"DEBUG: Checking for mindfulness activities for emotion '{emotionString}' (ID: {emotionValue})");

            try
            {
                var response = await supabaseClient
                    .From<MindfulnessActivity>()
                    .Filter(
                        "associated_emotions",
                        Operator.Contains,
                        new List<int> { emotionValue }
                    )
                    .Get();

                bool hasActivities = response.Models != null && response.Models.Count > 0;
                Console.WriteLine($"DEBUG: Found {response.Models?.Count ?? 0} mindfulness activities for '{emotionString}'");

                return hasActivities;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Exception checking mindfulness activities for '{emotionString}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if mindfulness activities exist for workout flow (excludes Happy/Energized)
        /// </summary>
        /// <param name="emotionString">The emotion string (e.g., "Happy", "Sad", etc.)</param>
        /// <returns>True if mindfulness activities exist and emotion should have mindfulness in workout flow</returns>
        public async Task<bool> HasWorkoutMindfulnessActivitiesForEmotion(string emotionString)
        {
            // For workout flow, Happy and Energized should always skip mindfulness
            if (emotionString == "Happy" || emotionString == "Energized")
            {
                Console.WriteLine($"DEBUG: WORKOUT FLOW - '{emotionString}' always skips mindfulness");
                return false;
            }

            // For other emotions, use the regular database check
            return await HasMindfulnessActivitiesForEmotion(emotionString);
        }





        /// <summary>
        /// Gets workout details by workout ID, including exercise information
        /// </summary>
        /// <param name="workoutId">The ID of the workout to fetch</param>
        /// <returns>Workout object with exercise details</returns>
        public async Task<Workout?> GetWorkoutById(int workoutId)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null)
            {
                Console.WriteLine($"GetWorkoutById({workoutId}) - supabaseClient is null");
                return null;
            }

            try
            {
                // First, try simplified query to avoid JSON parsing issues
                var simpleQuery = supabaseClient.From<Workout>()
                    .Select("workout_id, emotion_id, at_gym, category, impact")
                    .Where(w => w.WorkoutId == workoutId);

                var simpleResponse = await simpleQuery.Get();

                if (simpleResponse?.Models?.FirstOrDefault() == null)
                {
                    Console.WriteLine($"GetWorkoutById({workoutId}) - No workout found with this ID");
                    return null;
                }

                // Now get the full workout data separately to handle JSON properly
                var fullQuery = supabaseClient.From<Workout>()
                    .Where(w => w.WorkoutId == workoutId);
                var fullResponse = await fullQuery.Get();

                if (fullResponse?.Models?.FirstOrDefault() != null)
                {
                    var fullWorkout = fullResponse.Models.First();
                    var sectionCount = fullWorkout.Exercises?.Sections?.Count ?? 0;
                    var exerciseCount = fullWorkout.Exercises?.Exercises?.Count ?? 0;
                    Console.WriteLine($"GetWorkoutById({workoutId}) - Successfully loaded workout with {sectionCount} sections, {exerciseCount} direct exercises");

                    // Debug: Log exercises JSON structure
                    if (fullWorkout.Exercises != null)
                    {
                        Console.WriteLine($"DEBUG: Exercises object exists, Description: '{fullWorkout.Exercises.Description ?? "null"}'");
                        Console.WriteLine($"DEBUG: Sections is null? {fullWorkout.Exercises.Sections == null}, Exercises is null? {fullWorkout.Exercises.Exercises == null}");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: Exercises object is null for workout {workoutId}");
                    }

                    return fullWorkout;
                }
                else
                {
                    Console.WriteLine($"GetWorkoutById({workoutId}) - Full query failed, returning basic workout");
                    return simpleResponse.Models.First();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetWorkoutById({workoutId}): {ex.Message}");
                return null;
            }
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
        public async Task<List<WorkoutLogViewModel>> GetWorkoutLogsWithDetails(string? memberId = null)
        {
            await EnsureInitializedAsync();

            var result = new List<WorkoutLogViewModel>();
            if (supabaseClient == null) return result;

            try
            {
                // explicitly define the query type to avoid the compiler error
                Supabase.Postgrest.Interfaces.IPostgrestTable<WorkoutLog> query = supabaseClient.From<WorkoutLog>();

                if (!string.IsNullOrEmpty(memberId))
                {
                    Console.WriteLine($"Filtering workout logs by member ID: {memberId}");
                    query = query.Where(log => log.MemberId == memberId);
                }
                else
                {
                    Console.WriteLine("Getting ALL workout logs from database (no memberId specified)");
                }
                var workoutLogsResponse = await query.Get();

                Console.WriteLine($"Retrieved {workoutLogsResponse?.Models?.Count ?? 0} workout logs from database");

                if (workoutLogsResponse?.Models != null)
                {
                    foreach (var log in workoutLogsResponse.Models)
                    {
                        var viewModel = new WorkoutLogViewModel(log);

                        // Get workout details only if the log has a workout ID (excluding problematic info column)
                        if (log.WorkoutId.HasValue && log.WorkoutId.Value > 0)
                        {
                            try
                            {
                                Console.WriteLine($"Loading workout details for log {log.LogId} with workout ID {log.WorkoutId}");
                                var workoutResponse = await supabaseClient.From<Workout>()
                                    .Select("workout_id, emotion_id, category, at_gym, impact, exercises")
                                    .Where(w => w.WorkoutId == log.WorkoutId.Value)
                                    .Get();

                                viewModel.WorkoutDetails = workoutResponse?.Models?.FirstOrDefault();

                                if (viewModel.WorkoutDetails != null)
                                {
                                    Console.WriteLine($"Successfully loaded workout details: Category={viewModel.WorkoutDetails.Category}, EmotionId={viewModel.WorkoutDetails.EmotionId}");
                                }
                                else
                                {
                                    Console.WriteLine($"No workout found with ID {log.WorkoutId}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading workout details for log {log.LogId}: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Log {log.LogId} has no workout ID - likely a rest day entry");
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
                    .Select("workout_id, emotion_id, category, category_num, at_gym, impact, exercises")
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
                        .Where(log => log.WorkoutId.HasValue)
                        .GroupBy(log => log.WorkoutId.Value)
                        .ToDictionary(g => g.Key, g => g.OrderByDescending(log => log.DateTime).First().DateTime) ?? new Dictionary<int, DateTime>();

                    foreach (var workout in workoutsResponse.Models.OrderBy(w => w.WorkoutId))
                    {
                        var viewModel = new WorkoutViewModel(workout);

                        if (workoutLogDates.ContainsKey(workout.WorkoutId))
                        {
                            viewModel.WorkoutDate = workoutLogDates[workout.WorkoutId];
                        }

                        result.Add(viewModel);
                        Console.WriteLine($"Added workout: ID={workout.WorkoutId}, Category={workout.Category}, Date={viewModel.DisplayDate}, Equipment={workout.EquipmentType}");
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
        /// Gets workouts that the user has completed (has workout log entries for)
        /// </summary>
        /// <param name="memberId">Member ID to filter by</param>
        /// <returns>List of WorkoutViewModel for completed workouts</returns>
        public async Task<List<WorkoutViewModel>> GetCompletedWorkoutsForUser(string? memberId = null)
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
                // Get workout logs for the user (or all logs if no memberId provided)
                Console.WriteLine($"Querying workout logs for member: {memberId ?? "ALL"}");

                var query = supabaseClient.From<WorkoutLog>()
                    .Select("workout_id, date");

                if (!string.IsNullOrEmpty(memberId))
                {
                    query = query.Where(log => log.MemberId == memberId);
                }

                var workoutLogsResponse = await query.Get();
                Console.WriteLine($"Workout logs response received. Models count: {workoutLogsResponse?.Models?.Count ?? 0}");

                if (workoutLogsResponse?.Models?.Count > 0)
                {
                    // Get unique workout IDs that have been completed
                    var completedWorkoutIds = workoutLogsResponse.Models
                        .Where(log => log.WorkoutId.HasValue)
                        .Select(log => log.WorkoutId!.Value)
                        .Distinct()
                        .ToList();

                    Console.WriteLine($"Found {completedWorkoutIds.Count} unique completed workout IDs: {string.Join(", ", completedWorkoutIds)}");

                    // Get workout details for each completed workout
                    foreach (var workoutId in completedWorkoutIds)
                    {
                        try
                        {
                            Console.WriteLine($"Loading details for workout ID: {workoutId}");

                            // Use the existing GetWorkoutById method which handles JSON parsing
                            var workout = await GetWorkoutById(workoutId);

                            if (workout != null)
                            {
                                var viewModel = new WorkoutViewModel(workout);

                                // Set the date to the most recent workout log date for this workout
                                var mostRecentLog = workoutLogsResponse.Models
                                    .Where(log => log.WorkoutId == workoutId)
                                    .OrderByDescending(log => log.DateTime)
                                    .FirstOrDefault();

                                if (mostRecentLog != null)
                                {
                                    viewModel.WorkoutDate = mostRecentLog.DateTime;
                                }

                                result.Add(viewModel);
                                Console.WriteLine($"Successfully added workout {workoutId} - {workout.Category}");
                            }
                            else
                            {
                                Console.WriteLine($"Could not load workout details for ID: {workoutId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading workout {workoutId}: {ex.Message}");
                            // Continue with other workouts
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No workout logs found for user");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCompletedWorkoutsForUser: {ex.Message}");
            }

            // Sort by date, most recent first
            result = result.OrderByDescending(w => w.WorkoutDate).ToList();

            Console.WriteLine($"Returning {result.Count} completed workouts");
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
            // This is the old/unused method. We can ignore it, but I'll update it for completeness.
            await EnsureInitializedAsync();
            try
            {
                string? memberId = GetAuthenticatedMemberId();
                if (string.IsNullOrEmpty(memberId))
                {
                    Console.WriteLine("ATTN: Error in UploadJournalEntry - User not logged in.");
                    return;
                }

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
        public async Task<WorkoutLog?> GetTodaysWorkoutLog(string memberId)
        {
            // Make sure the client is ready
            await EnsureInitializedAsync();
            if (supabaseClient == null)
            {
                Console.WriteLine("Supabase client is not initialized.");
                return null;
            }

            // Get the start of the user's local day
            var localToday = DateTime.Today;
            // Get the start of the user's next local day
            var localTomorrow = localToday.AddDays(1);

            try
            {
                Console.WriteLine($"DEBUG: GetTodaysWorkoutLog - Looking for logs between {localToday} and {localTomorrow} for member {memberId}");

                // Query the 'workout_log' table, ordered by date_time descending to get the most recent entry
                var response = await supabaseClient.From<WorkoutLog>()
                    .Where(log => log.MemberId == memberId)
                    .Where(log => log.DateTime >= localToday)
                    .Where(log => log.DateTime < localTomorrow)
                    .Order(log => log.DateTime, Ordering.Descending) // Get the most recent entry first
                    .Limit(1) // We want the most recent log for today
                    .Get();

                var todaysLog = response.Models.FirstOrDefault();
                if (todaysLog != null)
                {
                    Console.WriteLine($"DEBUG: GetTodaysWorkoutLog - Found log ID: {todaysLog.LogId}, DateTime: {todaysLog.DateTime}");
                }
                else
                {
                    Console.WriteLine($"DEBUG: GetTodaysWorkoutLog - No log found for today");
                }

                // Return the most recent log for today, or null if none exist
                return todaysLog;
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
        public async Task<WorkoutLog?> CreateInitialWorkoutLog(string memberId, string beforeJournalText)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return null;

            try
            {
                var newLog = new WorkoutLog
                {
                    MemberId = memberId,
                    BeforeJournal = beforeJournalText,
                    DateTime = DateTime.Now // Use local time
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
        public async Task UpdateAfterJournalAsync(string logId, string afterJournalText)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null)
            {
                Console.WriteLine("ERROR: UpdateAfterJournalAsync - supabaseClient is null");
                return;
            }

            try
            {
                Console.WriteLine($"DEBUG: UpdateAfterJournalAsync - Updating log ID '{logId}' with after_journal text (length: {afterJournalText?.Length ?? 0})");

                // We find the log by its 'log_id' and update only the 'after_journal' column
                var result = await supabaseClient.From<WorkoutLog>()
                                      .Where(log => log.LogId == logId)
                                      .Set(log => log.AfterJournal, afterJournalText)
                                      .Update();

                Console.WriteLine($"DEBUG: UpdateAfterJournalAsync - Update operation completed, affected {result?.Models?.Count ?? 0} records");

                // Verify the update worked by fetching the log again
                var updatedLogQuery = await supabaseClient.From<WorkoutLog>()
                    .Where(log => log.LogId == logId)
                    .Get();

                var updatedLog = updatedLogQuery?.Models?.FirstOrDefault();
                if (updatedLog != null)
                {
                    Console.WriteLine($"DEBUG: UpdateAfterJournalAsync - Verification: after_journal is now '{updatedLog.AfterJournal?.Substring(0, Math.Min(50, updatedLog.AfterJournal?.Length ?? 0))}...'");
                    Console.WriteLine($"DEBUG: UpdateAfterJournalAsync - Verification: after_journal is null/empty? {string.IsNullOrEmpty(updatedLog.AfterJournal)}");
                }
                else
                {
                    Console.WriteLine($"ERROR: UpdateAfterJournalAsync - Could not find updated log with ID '{logId}' for verification");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: UpdateAfterJournalAsync - Exception: {ex.Message}");
                Console.WriteLine($"ERROR: UpdateAfterJournalAsync - Stack trace: {ex.StackTrace}");
                throw; // Re-throw so calling code knows there was an error
            }
        }


        // This function updates an existing log with the 'workout_id'
        public async Task UpdateWorkoutIdAsync(string logId, int workoutId)
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

        /// <summary>
        /// Maps emotion strings to emotion IDs used in the database
        /// Updated to match the actual emotions table structure
        /// </summary>
        private static readonly Dictionary<string, int> EmotionMapping = new()
        {
            { "Happy", 1 },
            { "Neutral", 2 },
            { "Sad", 3 },
            { "Depressed", 4 },
            { "Energized", 5 },
            { "Anxious", 6 },
            { "Angry", 7 },
            { "Tired", 8 }
        };

        /// <summary>
        /// Gets distinct workout categories available for a specific emotion
        /// </summary>
        /// <param name="emotion">The emotion string (e.g., "Happy", "Sad")</param>
        /// <returns>List of available workout categories for that emotion</returns>
        public async Task<List<string>> GetWorkoutCategoriesByEmotion(string emotion)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return new List<string>();

            try
            {
                // Special case for Neutral: use both Happy and Sad workout categories
                List<int> emotionIds;
                if (emotion == "Neutral")
                {
                    emotionIds = new List<int> { 1, 3 }; // Happy (1) and Sad (3)
                    Console.WriteLine($"DEBUG: SPECIAL CASE - Getting workout categories for Neutral using Happy (1) and Sad (3) pools");
                }
                else
                {
                    // Regular case: use single emotion ID
                    if (!EmotionMapping.TryGetValue(emotion, out int emotionId))
                    {
                        Console.WriteLine($"Unknown emotion: {emotion}");
                        return new List<string>();
                    }
                    emotionIds = new List<int> { emotionId };
                }

                Console.WriteLine($"DEBUG: Getting workout categories for emotion '{emotion}' using emotion_ids=[{string.Join(",", emotionIds)}]");

                // Query for distinct categories across all emotion IDs
                var allCategories = new HashSet<string>();

                foreach (int currentEmotionId in emotionIds)
                {
                    var query = supabaseClient.From<Workout>()
                        .Select("category")
                        .Where(w => w.EmotionId == currentEmotionId);

                    var response = await query.Get();

                    if (response?.Models != null)
                    {
                        var categories = response.Models
                            .Select(w => w.Category)
                            .Where(c => !string.IsNullOrEmpty(c))
                            .ToList();

                        foreach (var category in categories)
                        {
                            allCategories.Add(category);
                        }
                    }
                }

                var finalCategories = allCategories.OrderBy(c => c).ToList();
                Console.WriteLine($"DEBUG: Found categories for {emotion}: {string.Join(", ", finalCategories)}");
                return finalCategories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Exception in GetWorkoutCategoriesByEmotion: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Checks if an emotion is considered 'bad' and should offer mindfulness activities
        /// </summary>
        /// <param name="emotion">The emotion string</param>
        /// <returns>True if emotion should offer mindfulness first</returns>
        public static bool IsNegativeEmotion(string emotion)
        {
            var negativeEmotions = new HashSet<string> { "Sad", "Depressed", "Tired", "Angry", "Anxious" };
            return negativeEmotions.Contains(emotion);
        }

        /// <summary>
        /// Checks if equipment selection is needed for a specific emotion and workout category
        /// </summary>
        /// <param name="emotion">The emotion string</param>
        /// <param name="workoutCategory">The workout category (e.g., "Cardio", "Strength Training")</param>
        /// <returns>True if equipment selection is needed, false if all workouts have NULL at_gym</returns>
        public async Task<bool> IsEquipmentSelectionNeeded(string emotion, string workoutCategory)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return false;

            try
            {
                // Get emotion ID from mapping
                if (!EmotionMapping.TryGetValue(emotion, out int emotionId))
                {
                    Console.WriteLine($"Unknown emotion: {emotion}");
                    return false;
                }

                Console.WriteLine($"DEBUG: Checking equipment need for emotion '{emotion}' (ID: {emotionId}), category '{workoutCategory}'");

                // Query for all at_gym values for this emotion+category combination
                var query = supabaseClient.From<Workout>()
                    .Select("at_gym")
                    .Where(w => w.EmotionId == emotionId)
                    .Where(w => w.Category == workoutCategory);

                var response = await query.Get();

                if (response?.Models != null && response.Models.Count > 0)
                {
                    // Check if any workout has non-null at_gym value
                    bool hasNonNullAtGym = response.Models.Any(w => w.AtGym != null);

                    Console.WriteLine($"DEBUG: Found {response.Models.Count} workouts for {emotion}+{workoutCategory}, equipment selection needed: {hasNonNullAtGym}");
                    return hasNonNullAtGym;
                }

                // If no workouts found, equipment selection is not needed
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Exception in IsEquipmentSelectionNeeded: {ex.Message}");
                return true; // Default to showing equipment selection if error
            }
        }

        /// <summary>
        /// Debug method to check actual database data without JSON parsing
        /// </summary>
        public async Task TestDatabaseQueries()
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return;

            Console.WriteLine("=== TESTING DATABASE QUERIES ===");

            try
            {
                // First, check basic workouts table without complex JSON
                var simpleQuery = supabaseClient.From<Models.Workout>()
                    .Select("workout_id, emotion_id, at_gym, category, impact");

                var simpleResponse = await simpleQuery.Get();
                Console.WriteLine($"Found {simpleResponse?.Models?.Count ?? 0} workouts with simple query");

                if (simpleResponse?.Models != null && simpleResponse.Models.Count > 0)
                {
                    foreach (var w in simpleResponse.Models.Take(5))
                    {
                        Console.WriteLine($"Workout: ID={w.WorkoutId}, Emotion={w.EmotionId}, AtGym={w.AtGym}, Category={w.Category}");
                    }

                    // Test specific filters
                    Console.WriteLine("\n--- Testing Emotion Filter (Energized = 5) ---");
                    var energizedQuery = supabaseClient.From<Models.Workout>()
                        .Select("workout_id, emotion_id, at_gym, category")
                        .Where(w => w.EmotionId == 5);
                    var energizedResponse = await energizedQuery.Get();
                    Console.WriteLine($"Found {energizedResponse?.Models?.Count ?? 0} workouts for Energized (emotion_id=5)");

                    Console.WriteLine("\n--- Testing Gym Filter (at_gym = true) ---");
                    var gymQuery = supabaseClient.From<Models.Workout>()
                        .Select("workout_id, emotion_id, at_gym, category")
                        .Where(w => w.AtGym == true);
                    var gymResponse = await gymQuery.Get();
                    Console.WriteLine($"Found {gymResponse?.Models?.Count ?? 0} workouts for gym access");

                    Console.WriteLine("\n--- Testing Combined Filter (Energized + Gym) ---");
                    var combinedQuery = supabaseClient.From<Models.Workout>()
                        .Select("workout_id, emotion_id, at_gym, category")
                        .Where(w => w.EmotionId == 5)
                        .Where(w => w.AtGym == true);
                    var combinedResponse = await combinedQuery.Get();
                    Console.WriteLine($"Found {combinedResponse?.Models?.Count ?? 0} workouts for Energized + Gym");

                    if (combinedResponse?.Models != null)
                    {
                        foreach (var w in combinedResponse.Models)
                        {
                            Console.WriteLine($"Match: ID={w.WorkoutId}, Emotion={w.EmotionId}, AtGym={w.AtGym}, Category={w.Category}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No workouts found in database!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TestDatabaseQueries: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets workouts filtered by emotion, equipment availability, and workout type
        /// </summary>
        /// <param name="emotion">The user's current emotion/mood</param>
        /// <param name="hasGymAccess">Whether user has access to gym equipment</param>
        /// <param name="workoutType">The type of workout (Strength Training or Cardio)</param>
        /// <returns>List of matching workouts</returns>
        public async Task<List<Workout>> GetWorkoutsByEmotionAndEquipment(string emotion, bool hasGymAccess, string workoutType = "Strength Training")
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return new List<Workout>();

            try
            {
                // Special case for Neutral: use both Happy and Sad workouts
                List<int> emotionIds;
                if (emotion == "Neutral")
                {
                    emotionIds = new List<int> { 1, 3 }; // Happy (1) and Sad (3)
                    Console.WriteLine($"DEBUG: SPECIAL CASE - Neutral emotion will use Happy (1) and Sad (3) workout pools");
                }
                else
                {
                    // Regular case: use single emotion ID
                    if (!EmotionMapping.TryGetValue(emotion, out int emotionId))
                    {
                        Console.WriteLine($"Unknown emotion: {emotion}. Using default emotion ID 2 (Neutral)");
                        emotionId = 2; // Default to neutral
                    }
                    emotionIds = new List<int> { emotionId };
                }

                Console.WriteLine($"DEBUG: Querying workouts for emotion='{emotion}', emotion_ids=[{string.Join(",", emotionIds)}], gym_access={hasGymAccess}, workout_type='{workoutType}'");

                // Query each emotion ID separately and combine results
                var combinedWorkouts = new List<Workout>();

                foreach (int currentEmotionId in emotionIds)
                {
                    Console.WriteLine($"DEBUG: Querying emotion_id {currentEmotionId}");

                    if (workoutType == "Strength Training")
                    {
                        Console.WriteLine($"DEBUG: Filtering by gym access: {hasGymAccess}");

                        try
                        {
                            // Query all strength training workouts for this emotion first
                            var allStrengthQuery = supabaseClient.From<Workout>()
                                .Select("*")  // Select all fields to get complete workout data
                                .Where(w => w.EmotionId == currentEmotionId)
                                .Where(w => w.Category == workoutType);

                            var allResponse = await allStrengthQuery.Get();
                            Console.WriteLine($"DEBUG: Retrieved {allResponse?.Models?.Count ?? 0} strength training workouts for emotion_id {currentEmotionId}");

                            if (allResponse?.Models != null)
                            {
                                // Filter client-side based on gym access preference
                                // Include workouts where at_gym matches preference OR is null (works anywhere)
                                var filtered = allResponse.Models.Where(w =>
                                    w.AtGym == hasGymAccess || w.AtGym == null
                                ).ToList();

                                combinedWorkouts.AddRange(filtered);
                                Console.WriteLine($"DEBUG: Found {filtered.Count} strength workouts matching gym preference (hasGym={hasGymAccess})");

                                // Debug: show what we filtered
                                foreach (var workout in filtered)
                                {
                                    Console.WriteLine($"DEBUG: - Workout {workout.WorkoutId}: at_gym={workout.AtGym}");
                                }
                            }
                        }
                        catch (Exception queryEx)
                        {
                            Console.WriteLine($"DEBUG: Error in strength training query: {queryEx.Message}");
                            // Continue to next emotion if this one fails
                        }
                    }
                    else // Cardio workouts don't need gym filtering
                    {
                        try
                        {
                            var cardioQuery = supabaseClient.From<Workout>()
                                .Select("*")  // Select all fields to get complete workout data
                                .Where(w => w.EmotionId == currentEmotionId)
                                .Where(w => w.Category == workoutType);

                            var cardioResponse = await cardioQuery.Get();
                            Console.WriteLine($"DEBUG: Retrieved {cardioResponse?.Models?.Count ?? 0} cardio workouts for emotion_id {currentEmotionId}");

                            if (cardioResponse?.Models != null)
                            {
                                combinedWorkouts.AddRange(cardioResponse.Models);
                                Console.WriteLine($"DEBUG: Added {cardioResponse.Models.Count} cardio workouts to combined list");
                            }
                        }
                        catch (Exception cardioEx)
                        {
                            Console.WriteLine($"DEBUG: Error in cardio query: {cardioEx.Message}");
                            // Continue to next emotion if this one fails
                        }
                    }
                }

                Console.WriteLine($"DEBUG: Combined query found {combinedWorkouts.Count} matching workouts");
                Console.WriteLine($"DEBUG: Returning {combinedWorkouts.Count} complete workouts directly from query");

                return combinedWorkouts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Exception in GetWorkoutsByEmotionAndEquipment: {ex.Message}");
                Console.WriteLine($"ERROR: Stack trace: {ex.StackTrace}");
                return new List<Workout>();
            }
        }

        /// <summary>
        /// Selects a random workout from the filtered results (strict mode - no fallbacks)
        /// </summary>
        /// <param name="emotion">The user's current emotion/mood</param>
        /// <param name="hasGymAccess">Whether user has access to gym equipment</param>
        /// <param name="workoutType">The type of workout (Strength Training or Cardio)</param>
        /// <returns>A randomly selected workout matching exact criteria or null if none found</returns>
        public async Task<Workout?> GetRandomWorkoutByExactCriteria(string emotion, bool hasGymAccess, string workoutType)
        {
            var workouts = await GetWorkoutsByEmotionAndEquipment(emotion, hasGymAccess, workoutType);

            if (workouts.Count == 0)
            {
                Console.WriteLine($"DEBUG: No workouts available for exact criteria - emotion='{emotion}', gym_access={hasGymAccess}, type='{workoutType}'");
                return null; // No fallbacks - return null if exact criteria not met
            }

            // Filter out workouts that don't have exercise sections or exercises
            var workoutsWithExercises = workouts.Where(w =>
                (w.Exercises?.Sections?.Count > 0) ||
                (w.Exercises?.Exercises?.Count > 0)
            ).ToList();

            Console.WriteLine($"DEBUG: Found {workouts.Count} total workouts, {workoutsWithExercises.Count} have exercises for exact criteria");

            if (workoutsWithExercises.Count == 0)
            {
                Console.WriteLine($"DEBUG: No workouts with exercises found for exact criteria");
                return null; // No fallbacks
            }

            // Select random workout from those with exercises
            var random = new Random();
            var selectedWorkout = workoutsWithExercises[random.Next(workoutsWithExercises.Count)];

            var sectionCount = selectedWorkout.Exercises?.Sections?.Count ?? 0;
            var exerciseCount = selectedWorkout.Exercises?.Exercises?.Count ?? 0;
            Console.WriteLine($"DEBUG: Selected workout ID: {selectedWorkout.WorkoutId} - {selectedWorkout.Category} with {sectionCount} sections, {exerciseCount} exercises (exact criteria)");
            return selectedWorkout;
        }

        /// <summary>
        /// Selects a random workout from the filtered results (with fallbacks for backward compatibility)
        /// </summary>
        /// <param name="emotion">The user's current emotion/mood</param>
        /// <param name="hasGymAccess">Whether user has access to gym equipment</param>
        /// <param name="workoutType">The type of workout (Strength Training or Cardio)</param>
        /// <returns>A randomly selected workout or null if none found</returns>
        public async Task<Workout?> GetRandomWorkoutByEmotionAndEquipment(string emotion, bool hasGymAccess, string workoutType = "Strength Training")
        {
            var workouts = await GetWorkoutsByEmotionAndEquipment(emotion, hasGymAccess, workoutType);

            if (workouts.Count == 0)
            {
                Console.WriteLine($"DEBUG: No workouts available for emotion='{emotion}', gym_access={hasGymAccess}");

                // Try alternative approaches before fallback
                Console.WriteLine("DEBUG: Trying to find ANY workout for this emotion...");
                var emotionOnlyWorkouts = await GetWorkoutsByEmotionOnly(emotion);
                var emotionWorkoutsWithSections = emotionOnlyWorkouts.Where(w => w.Exercises?.Sections?.Count > 0).ToList();

                if (emotionWorkoutsWithSections.Count > 0)
                {
                    Console.WriteLine($"DEBUG: Found {emotionOnlyWorkouts.Count} workouts for emotion '{emotion}', {emotionWorkoutsWithSections.Count} have sections");
                    var randomGenerator = new Random();
                    var emotionWorkout = emotionWorkoutsWithSections[randomGenerator.Next(emotionWorkoutsWithSections.Count)];
                    Console.WriteLine($"DEBUG: Selected emotion-based workout ID: {emotionWorkout.WorkoutId} - {emotionWorkout.Category} with {emotionWorkout.Exercises?.Sections?.Count} sections");
                    return emotionWorkout;
                }

                // Final fallback: try to get ANY workout with sections
                Console.WriteLine("DEBUG: Trying final fallback - any workout with sections...");
                var anyWorkoutWithSections = await GetAnyWorkoutWithSections();
                if (anyWorkoutWithSections != null)
                {
                    Console.WriteLine($"DEBUG: Using final fallback workout ID: {anyWorkoutWithSections.WorkoutId} with {anyWorkoutWithSections.Exercises?.Sections?.Count} sections");
                    return anyWorkoutWithSections;
                }

                // Fallback: try to create a simple workout for testing
                Console.WriteLine("DEBUG: Creating fallback workout for testing...");
                return CreateFallbackWorkout(emotion, hasGymAccess);
            }

            // Filter out workouts that don't have exercises (either sections or exercises array)
            var workoutsWithExercises = workouts.Where(w =>
                (w.Exercises?.Sections?.Count > 0) ||
                (w.Exercises?.Exercises?.Count > 0)
            ).ToList();

            Console.WriteLine($"DEBUG: Found {workouts.Count} total workouts, {workoutsWithExercises.Count} have exercises");

            if (workoutsWithExercises.Count == 0)
            {
                Console.WriteLine("DEBUG: No workouts with sections found! Using fallback...");
                // Try to get ANY workout with sections as fallback
                var fallbackWithSections = await GetAnyWorkoutWithSections();
                if (fallbackWithSections != null)
                {
                    Console.WriteLine($"DEBUG: Using fallback workout ID: {fallbackWithSections.WorkoutId} with {fallbackWithSections.Exercises?.Sections?.Count} sections");
                    return fallbackWithSections;
                }

                Console.WriteLine("DEBUG: No workouts with sections exist in database!");
                return null; // Don't return empty workouts
            }

            // Select random workout from those with exercises
            var random = new Random();
            var selectedWorkout = workoutsWithExercises[random.Next(workoutsWithExercises.Count)];

            var sectionCount = selectedWorkout.Exercises?.Sections?.Count ?? 0;
            var exerciseCount = selectedWorkout.Exercises?.Exercises?.Count ?? 0;
            Console.WriteLine($"DEBUG: Selected workout ID: {selectedWorkout.WorkoutId} - {selectedWorkout.Category} with {sectionCount} sections, {exerciseCount} exercises");
            return selectedWorkout;
        }

        /// <summary>
        /// Gets any workout that has exercise sections (fallback method)
        /// </summary>
        /// <returns>A workout with sections or null if none exist</returns>
        private async Task<Workout?> GetAnyWorkoutWithSections()
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return null;

            try
            {
                // Check the first few workouts to find one with sections
                for (int workoutId = 1; workoutId <= 50; workoutId++)
                {
                    var workout = await GetWorkoutById(workoutId);
                    if (workout?.Exercises?.Sections?.Count > 0)
                    {
                        Console.WriteLine($"DEBUG: Found fallback workout {workoutId} with {workout.Exercises.Sections.Count} sections");
                        return workout;
                    }
                }

                Console.WriteLine("DEBUG: No workouts with sections found in first 50 workouts");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error in GetAnyWorkoutWithSections: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets workouts filtered by emotion only (ignoring equipment)
        /// </summary>
        /// <param name="emotion">The user's current emotion/mood</param>
        /// <returns>List of matching workouts</returns>
        private async Task<List<Workout>> GetWorkoutsByEmotionOnly(string emotion)
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return new List<Workout>();

            try
            {
                // Get emotion ID from mapping
                if (!EmotionMapping.TryGetValue(emotion, out int emotionId))
                {
                    Console.WriteLine($"Unknown emotion: {emotion}. Using default emotion ID 2 (Neutral)");
                    emotionId = 2; // Default to neutral
                }

                Console.WriteLine($"DEBUG: Querying workouts for emotion='{emotion}', emotion_id={emotionId} (ignoring equipment)");

                var query = supabaseClient.From<Workout>()
                    .Select("workout_id, emotion_id, category, impact, at_gym")
                    .Where(w => w.EmotionId == emotionId);

                var response = await query.Get();
                Console.WriteLine($"DEBUG: Emotion-only query returned {response?.Models?.Count ?? 0} workouts");

                if (response?.Models != null && response.Models.Count > 0)
                {
                    // Now get the full workout data including exercises for the found workouts
                    var fullWorkouts = new List<Workout>();
                    foreach (var workout in response.Models)
                    {
                        try
                        {
                            var fullWorkoutQuery = supabaseClient.From<Workout>()
                                .Where(w => w.WorkoutId == workout.WorkoutId);
                            var fullWorkoutResponse = await fullWorkoutQuery.Get();
                            if (fullWorkoutResponse?.Models?.FirstOrDefault() != null)
                            {
                                fullWorkouts.Add(fullWorkoutResponse.Models.First());
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"DEBUG: Error loading full workout {workout.WorkoutId}: {ex.Message}");
                            // Add the partial workout anyway
                            fullWorkouts.Add(workout);
                        }
                    }

                    Console.WriteLine($"DEBUG: Successfully loaded {fullWorkouts.Count} full workouts for emotion only");
                    return fullWorkouts;
                }
                else
                {
                    Console.WriteLine($"DEBUG: No workouts found for emotion_id={emotionId}");
                    return new List<Workout>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Exception in GetWorkoutsByEmotionOnly: {ex.Message}");
                return new List<Workout>();
            }
        }

        /// <summary>
        /// Deletes the user's data (logs and profile) and signs them out.
        /// Note: This cleans up PUBLIC data. The Auth record remains until an Admin deletes it,
        /// but this satisfies App Store requirements for "User Initiated Deletion".
        /// </summary>
        /// <returns>Null if successful, error message otherwise</returns>
        public async Task<string?> DeleteAccount()
        {
            await EnsureInitializedAsync();
            if (supabaseClient == null) return "Database not initialized";

            try
            {
                // 1. Get the current User ID
                var userId = GetAuthenticatedMemberId();
                if (string.IsNullOrEmpty(userId))
                {
                    return "User is not currently logged in.";
                }

                Console.WriteLine($"DEBUG: Attempting to delete account data for {userId}");

                // 2. Delete Workout Logs (Data Cleanup)
                try
                {
                    await supabaseClient.From<WorkoutLog>()
                        .Where(x => x.MemberId == userId)
                        .Delete();
                    Console.WriteLine("DEBUG: Workout logs deleted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Could not delete workout logs (RLS Policy?): {ex.Message}");
                    // Continue anyway - do not stop the logout process
                }

                // 3. Delete Member Profile (Data Cleanup)
                try
                {
                    await supabaseClient.From<Member>()
                        .Where(x => x.MemberId == userId)
                        .Delete();
                    Console.WriteLine("DEBUG: Member profile deleted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Could not delete member profile (RLS Policy?): {ex.Message}");
                    // Continue anyway
                }

                // 4. Log Out (The "Kick")
                await LogOut();

                return null; // Success
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: DeleteAccount failed: {ex.Message}");
                // Even if the DB fails, force a local logout so the user feels "Deleted"
                await LogOut();
                return null; // We return success so the UI navigates away
            }
        }

        /// <summary>
        /// Creates a fallback workout for testing when database queries fail
        /// </summary>
        private Workout CreateFallbackWorkout(string emotion, bool hasGymAccess)
        {
            Console.WriteLine($"DEBUG: Creating fallback workout for {emotion} + {(hasGymAccess ? "Gym" : "Home")}");

            var fallbackWorkout = new Workout
            {
                WorkoutId = 999, // Use a special ID for fallback
                EmotionId = EmotionMapping.GetValueOrDefault(emotion, 2),
                Category = "Fallback Strength Training",
                Impact = "High",
                AtGym = hasGymAccess,
                Exercises = new WorkoutExercises
                {
                    Description = $"A fallback workout generated for {emotion} emotion with {(hasGymAccess ? "gym" : "home")} equipment.",
                    Sections = new List<WorkoutSection>
                    {
                        new WorkoutSection
                        {
                            Type = "main",
                            Title = "Simple Workout",
                            Note = "This is a fallback workout for testing purposes.",
                            Exercises = new List<WorkoutExerciseItem>
                            {
                                new WorkoutExerciseItem { Id = 1, SetsRaw = 3, Reps = "10", Rest = "60 seconds" },
                                new WorkoutExerciseItem { Id = 2, SetsRaw = 3, Reps = "12", Rest = "60 seconds" }
                            }
                        }
                    }
                }
            };

            return fallbackWorkout;
        }
    }
}
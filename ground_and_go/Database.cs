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
        public Dictionary<int, Exercise>? ExercisesDictionary { get; set; }
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
            int memberId;
            var response = await supabaseClient.From<Member>().Where(member => member.Email == userEmail).Get();

            if (response.Models.Any()) //If the query returned any rows
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
            var response = await supabaseClient.From<WorkoutLog>().Where(workoutLog => workoutLog.MemberId == memberId).Get();
            if (response.Models.Any())
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
            //THIS METHOD DOES NOT WORK CURRENTLY
            await EnsureInitializedAsync();
            var response = await supabaseClient.From<Exercise>().Get();
            foreach (Exercise row in response.Models)
            {
                ExercisesDictionary.Add(row.ExerciseId, row);
            }
        }


    }
}
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



    }
}
namespace ground_and_go.Services
{
    public class MockAuthService
    {
        // This is our fake, hard-coded user ID.
        // We'll pretend user "1" is always logged in.
        // Your "login guy" will eventually replace this logic.
        public int GetCurrentMemberId()
        {
            return 1; 
        }
    }
}
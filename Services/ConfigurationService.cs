namespace ground_and_go.Services
{
    public class ConfigurationService
    {
        private readonly Dictionary<string, string> _settings;

        public ConfigurationService()
        {
            _settings = new Dictionary<string, string>();
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            // NOTE: In production/deployment, these values would be loaded from environment variables
            // and this ConfigurationService.cs file would be added to .gitignore for security
            // For class project evaluation purposes, values are included here for easy grading access
            
            _settings["Supabase:Url"] = "https://irekjohmgsjicpszbgus.supabase.co";
            _settings["Supabase:ApiKey"] = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImlyZWtqb2htZ3NqaWNwc3piZ3VzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjEwODcwMTQsImV4cCI6MjA3NjY2MzAxNH0.vAMY-0u9u2hNbGIcg4h7tdhI6cOW5jUMcMWEP67ChxQ";
            _settings["Features:DefaultVideoUrl"] = "https://www.youtube.com/shorts/hWbUlkb5Ms4";
        }

        public string GetSupabaseUrl()
        {
            return _settings.GetValueOrDefault("Supabase:Url", "https://irekjohmgsjicpszbgus.supabase.co");
        }

        public string GetSupabaseApiKey()
        {
            return _settings.GetValueOrDefault("Supabase:ApiKey", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImlyZWtqb2htZ3NqaWNwc3piZ3VzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjEwODcwMTQsImV4cCI6MjA3NjY2MzAxNH0.vAMY-0u9u2hNbGIcg4h7tdhI6cOW5jUMcMWEP67ChxQ");
        }

        public string GetDefaultVideoUrl()
        {
            return _settings.GetValueOrDefault("Features:DefaultVideoUrl", "https://www.youtube.com/shorts/hWbUlkb5Ms4");
        }

        public string GetValue(string key)
        {
            return _settings.GetValueOrDefault(key, "");
        }
    }
}
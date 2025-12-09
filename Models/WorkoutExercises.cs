using Newtonsoft.Json;

namespace ground_and_go.Models
{
    public class WorkoutExercises
    {
        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("sections")]
        public List<WorkoutSection> Sections { get; set; } = new();

        [JsonProperty("exercises")]
        public List<WorkoutExerciseItem> Exercises { get; set; } = new();
    }

    public class WorkoutSection
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("title")]
        public string Title { get; set; } = "";

        [JsonProperty("note")]
        public string Note { get; set; } = "";

        [JsonProperty("exercises")]
        public List<WorkoutExerciseItem> Exercises { get; set; } = new();

        [JsonProperty("sets")]
        public string? Sets { get; set; }

        [JsonProperty("rounds")]
        public WorkoutRounds? Rounds { get; set; }

        [JsonProperty("rest_between_rounds")]
        public string? RestBetweenRounds { get; set; }

        [JsonProperty("rest_between_exercises")]
        public string? RestBetweenExercises { get; set; }
    }

    public class WorkoutExerciseItem
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("sets")]
        public object? SetsRaw { get; set; }

        [JsonProperty("reps")]
        public string? Reps { get; set; }

        [JsonProperty("rest")]
        public string? Rest { get; set; }

        [JsonProperty("duration")]
        public string? Duration { get; set; }

        [JsonProperty("note")]
        public string? Note { get; set; }

        // Helper property to get sets as integer or null
        public int? Sets 
        { 
            get 
            {
                if (SetsRaw is int intValue) return intValue;
                if (SetsRaw is string strValue && int.TryParse(strValue, out int parsed)) return parsed;
                return null;
            }
        }

        // Helper property to get sets as display string
        public string? SetsDisplay 
        { 
            get => SetsRaw?.ToString(); 
        }
    }

    public class WorkoutRounds
    {
        [JsonProperty("min")]
        public int Min { get; set; }

        [JsonProperty("max")]
        public int Max { get; set; }
    }
}
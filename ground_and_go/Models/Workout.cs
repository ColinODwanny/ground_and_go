using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace ground_and_go.Models
{

    [Table("workouts")]
    public class Workout: ObservableBaseModel, IEquatable<Workout>
    {
        int _workoutId = -1;
        int _emotion_id = -1;
        string _category = "";
        int _category_num = -1;
        string _equipment = "";
        string _impact = "";
        string _info = "";
        int[] _exercies = [];

        [PrimaryKey("workout_id", true)]
        public int WorkoutId
        {
            get => _workoutId;
        }

        [Column("emotion_id")]
        public int EmotionId
        {
            get => _emotion_id;
        }

        [Column("category")]
        public string Category
        {
            get => _category;
        }

        [Column("category_num")]
        public int CategoryNum
        {
            get => _category_num;
        }

        [Column("equipment")]
        public string Equipment
        {
            get => _equipment;
        }

        [Column("impact")]
        public string Impact
        {
            get => _impact;
        }

        [Column("info")]
        public string Info
        {
            get => _info;
        }

        [Column("exercises")]
        public int[] Exercises
        {
            get => _exercies;
        }

        public override bool Equals(object? obj)
            => obj is Workout other && WorkoutId == other.WorkoutId;

        public bool Equals(Workout? other)
            => other is not null && WorkoutId == other.WorkoutId;

        public override int GetHashCode()
            => WorkoutId.GetHashCode();
    }
}
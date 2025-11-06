using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Linq;

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
            set => SetProperty(ref _workoutId, value);
        }

        [Column("emotion_id")]
        public int EmotionId
        {
            get => _emotion_id;
            set => SetProperty(ref _emotion_id, value);
        }

        [Column("category")]
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        [Column("category_num")]
        public int CategoryNum
        {
            get => _category_num;
            set => SetProperty(ref _category_num, value);
        }

        [Column("equipment")]
        public string Equipment
        {
            get => _equipment;
            set => SetProperty(ref _equipment, value);
        }

        [Column("impact")]
        public string Impact
        {
            get => _impact;
            set => SetProperty(ref _impact, value);
        }

        [Column("info")]
        public string Info
        {
            get => _info;
            set => SetProperty(ref _info, value);
        }

        [Column("exercises")]
        public int[] Exercises
        {
            get => _exercies;
            set => SetProperty(ref _exercies, value);
        }



        [JsonIgnore]
        public string ExercisesBulletedList =>
            Exercises != null && Exercises.Length > 0 && Database.ExercisesDictionary?.Count > 0
                ? string.Join(Environment.NewLine,
                    Exercises.Select(id =>
                        Database.ExercisesDictionary.TryGetValue(id, out var exercise)
                            ? $"• {exercise.Name}"
                            : $"• Unknown ({id})"
                    ))
                : "No exercises";


        public override bool Equals(object? obj)
            => obj is Workout other && WorkoutId == other.WorkoutId;

        public bool Equals(Workout? other)
            => other is not null && WorkoutId == other.WorkoutId;

        public override int GetHashCode()
            => WorkoutId.GetHashCode();
    }
}
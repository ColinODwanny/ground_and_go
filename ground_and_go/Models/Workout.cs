using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
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
        string _impact = "";
        bool? _atGym = null;
        WorkoutExercises _exercises = new();

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

        [Column("at_gym")]
        public bool? AtGym
        {
            get => _atGym;
            set => SetProperty(ref _atGym, value);
        }

        [Column("impact")]
        public string Impact
        {
            get => _impact;
            set => SetProperty(ref _impact, value);
        }

        [Column("exercises")]
        public WorkoutExercises Exercises
        {
            get => _exercises;
            set => SetProperty(ref _exercises, value);
        }



        [JsonIgnore]
        public string WorkoutDescription => Exercises?.Description ?? "No description available";
        
        [JsonIgnore]
        public string EquipmentType => AtGym == true ? "Gym" : AtGym == false ? "Home" : "Any Location";
        
        // Backward compatibility properties for WorkoutViewModel
        [JsonIgnore]
        public string Equipment => EquipmentType;
        
        [JsonIgnore]
        public string Info => WorkoutDescription;
        
        [JsonIgnore]
        public string ExercisesBulletedList => GetExercisesBulletedList();
        
        private string GetExercisesBulletedList()
        {
            if (Exercises?.Sections == null || Exercises.Sections.Count == 0)
                return "No exercises available";
            
            var sectionInfo = new List<string>();
            
            foreach (var section in Exercises.Sections)
            {
                if (section.Exercises != null && section.Exercises.Count > 0)
                {
                    var exerciseDetails = section.Exercises.Select(ex => 
                        $"  - Exercise #{ex.Id}" + 
                        (string.IsNullOrEmpty(ex.SetsDisplay) ? "" : $" ({ex.SetsDisplay} sets)") +
                        (string.IsNullOrEmpty(ex.Reps) ? "" : $" x {ex.Reps} reps")).ToList();
                    
                    sectionInfo.Add($"• {section.Title ?? "Unknown Section"} ({section.Exercises.Count} exercises)");
                    sectionInfo.AddRange(exerciseDetails);
                }
            }
            
            return sectionInfo.Count > 0 ? string.Join(Environment.NewLine, sectionInfo) : "No exercises available";
        }


        public override bool Equals(object? obj)
            => obj is Workout other && WorkoutId == other.WorkoutId;

        public bool Equals(Workout? other)
            => other is not null && WorkoutId == other.WorkoutId;

        public override int GetHashCode()
            => WorkoutId.GetHashCode();
    }
}
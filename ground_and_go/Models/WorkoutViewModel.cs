using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ground_and_go.Models
{
    public class WorkoutViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded = false;
        private DateTime? _workoutDate = null;

        public Workout Workout { get; set; }

        public DateTime? WorkoutDate
        {
            get => _workoutDate;
            set
            {
                _workoutDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayDate));
            }
        }
        
        public string ExercisesBulletedList => Workout?.ExercisesBulletedList ?? "No exercises";


        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExpandCollapseIcon));
            }
        }

        // public string ExpandCollapseIcon => IsExpanded ? "▼" : "▶";
        // public string ExpandCollapseIcon => IsExpanded ? "∇" : "∆"; 
        public string ExpandCollapseIcon => IsExpanded ? "▼\uFE0E" : "▶\uFE0E";
        
        public string DisplayDate => WorkoutDate?.ToString("MMM dd, yyyy") ?? "No date";
        
        public string WorkoutCategory => Workout.Category ?? "Unknown";
        
        public string WorkoutEquipment => Workout.Equipment ?? "N/A";
        
        public string ExercisesArray => GetExercisesDisplay();

        public string WorkoutInfo => string.IsNullOrEmpty(Workout.Info) ? "No description available" : Workout.Info;
        
        public string WorkoutEmotion => GetEmotionName();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WorkoutViewModel(Workout workout)
        {
            Workout = workout;
        }
        
        private string GetExercisesDisplay()
        {
            if (Workout?.Exercises?.Sections == null || Workout.Exercises.Sections.Count == 0)
                return "No exercises";
            
            var exerciseCount = 0;
            var sectionNames = new List<string>();
            
            foreach (var section in Workout.Exercises.Sections)
            {
                if (section.Exercises != null)
                {
                    exerciseCount += section.Exercises.Count;
                    sectionNames.Add(section.Title ?? "Unknown Section");
                }
            }
            
            return exerciseCount > 0 ? 
                $"{exerciseCount} exercises across {sectionNames.Count} sections" : 
                "No exercises";
        }
        
        private string GetEmotionName()
        {
            var emotionMap = new Dictionary<int, string>
            {
                { 1, "Happy" },
                { 2, "Neutral" },
                { 3, "Sad" },
                { 4, "Depressed" },
                { 5, "Energized" },
                { 6, "Anxious" },
                { 7, "Angry" },
                { 8, "Tired" }
            };
            
            return emotionMap.TryGetValue(Workout.EmotionId, out var emotion) ? emotion : "Unknown";
        }
    }
}
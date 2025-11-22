using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ground_and_go.Models
{
    public class WorkoutLogViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded = false;

        public WorkoutLog WorkoutLog { get; set; }
        public Workout? WorkoutDetails { get; set; }
        public ObservableCollection<Exercise> Exercises { get; set; } = new();

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

        public string ExpandCollapseIcon => IsExpanded ? "▼" : "▶";
        
        public string DisplayDate => WorkoutLog.DateTime.ToString("MMM dd, yyyy");
        
        public string WorkoutCategory => WorkoutDetails?.Category ?? "Unknown";
        
        public string WorkoutEquipment => WorkoutDetails?.Equipment ?? "N/A";
        
        public string ExercisesList => GetExercisesDisplay();
        
        public string WorkoutEmotion => GetEmotionName();
        
        public bool HasBeforeJournal => !string.IsNullOrEmpty(WorkoutLog.BeforeJournal);
        
        public bool HasAfterJournal => !string.IsNullOrEmpty(WorkoutLog.AfterJournal);

        public string BeforeJournalPreview => HasBeforeJournal ? 
            (WorkoutLog.BeforeJournal.Length > 100 ? 
                WorkoutLog.BeforeJournal.Substring(0, 100) + "..." : 
                WorkoutLog.BeforeJournal) : "";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WorkoutLogViewModel(WorkoutLog workoutLog)
        {
            WorkoutLog = workoutLog;
        }
        
        private string GetExercisesDisplay()
        {
            if (WorkoutDetails?.Exercises?.Sections == null || WorkoutDetails.Exercises.Sections.Count == 0)
                return "No exercises";
            
            var exerciseCount = 0;
            var sectionNames = new List<string>();
            
            foreach (var section in WorkoutDetails.Exercises.Sections)
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
            if (WorkoutDetails?.EmotionId == null) return "Unknown";
            
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
            
            return emotionMap.TryGetValue(WorkoutDetails.EmotionId, out var emotion) ? emotion : "Unknown";
        }
    }
}
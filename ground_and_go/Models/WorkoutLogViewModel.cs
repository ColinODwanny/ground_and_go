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
        
        public string ExercisesList => WorkoutDetails?.Exercises != null ? 
            $"[{string.Join(", ", WorkoutDetails.Exercises)}]" : "No exercises";
        
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
    }
}
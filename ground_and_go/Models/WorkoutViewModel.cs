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
        
        public string DisplayDate => WorkoutDate?.ToString("MMM dd, yyyy") ?? "No date";
        
        public string WorkoutCategory => Workout.Category ?? "Unknown";
        
        public string WorkoutEquipment => Workout.Equipment ?? "N/A";
        
        public string ExercisesArray => Workout.Exercises != null ? 
            $"[{string.Join(", ", Workout.Exercises)}]" : "No exercises";

        public string WorkoutInfo => string.IsNullOrEmpty(Workout.Info) ? "No description available" : Workout.Info;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WorkoutViewModel(Workout workout)
        {
            Workout = workout;
        }
    }
}
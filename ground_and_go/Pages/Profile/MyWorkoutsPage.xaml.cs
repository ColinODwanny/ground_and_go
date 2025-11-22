// FILE: ground_and_go/Pages/Profile/MyWorkoutsPage.xaml.cs
using ground_and_go.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ground_and_go.Pages.Profile;

public partial class MyWorkoutsPage : ContentPage, INotifyPropertyChanged
{
    private readonly Database _database;
    private bool _isLoading = false;
    
    public ObservableCollection<WorkoutViewModel> Workouts { get; set; } = new();
    
    public bool IsLoading 
    { 
        get => _isLoading; 
        set 
        { 
            _isLoading = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(IsNotLoading));
        } 
    }
    
    public bool IsNotLoading => !IsLoading;

    public MyWorkoutsPage(Database database)
    {
        InitializeComponent();
        _database = database;
        BindingContext = this;
        LoadWorkouts();
    }

    private async void LoadWorkouts()
    {
        try
        {
            IsLoading = true;
            
            // Get the current user's completed workouts
            string? memberId = _database.GetAuthenticatedMemberId();
            Console.WriteLine($"Loading workouts for member: {memberId}");
            
            var workouts = await _database.GetCompletedWorkoutsForUser(memberId);
            
            // Debug output
            Console.WriteLine($"Found {workouts.Count} completed workouts for user");
            
            Workouts.Clear();
            foreach (var workout in workouts)
            {
                try
                {
                    Workouts.Add(workout);
                    Console.WriteLine($"Added workout: ID={workout.Workout?.WorkoutId}, Category={workout.WorkoutCategory}, Date={workout.DisplayDate}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error adding workout {workout.Workout?.WorkoutId}: {ex.Message}");
                    // Continue adding other workouts even if one fails
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load workouts: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void OnWorkoutTapped(object sender, EventArgs e)
    {
        if (sender is Border border && border.BindingContext is WorkoutViewModel workout)
        {
            workout.IsExpanded = !workout.IsExpanded;
        }
    }
}
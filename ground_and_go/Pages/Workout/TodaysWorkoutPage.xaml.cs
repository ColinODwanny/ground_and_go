//Aidan Trusky
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class TodaysWorkoutPage : ContentPage
{
    private Dictionary<string, Border> exerciseBorders;

    public TodaysWorkoutPage()
    {
        InitializeComponent();
        
        // Initialize the dictionary to map exercise names to their buttons
        exerciseBorders = new Dictionary<string, Border>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Map exercise names to their corresponding borders
        exerciseBorders["Squat"] = SquatBorder;
        exerciseBorders["Bench Press"] = BenchPressBorder;
        exerciseBorders["Deadlift"] = DeadliftBorder;
        exerciseBorders["Pull-Up"] = PullUpBorder;
        exerciseBorders["Shoulder Press"] = ShoulderPressBorder;
    }

    private async void OnBeginExerciseClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;

            var exerciseName = button.CommandParameter?.ToString();
            if (string.IsNullOrEmpty(exerciseName)) return;

            // Show the exercise detail popup
            var popup = new ExerciseDetailPopup(exerciseName);
            var result = await this.ShowPopupAsync(popup);

            // If exercise was marked as complete, change the border color
            if (result is string completedExercise && exerciseBorders.ContainsKey(completedExercise))
            {
                exerciseBorders[completedExercise].BackgroundColor = Color.FromArgb("#C8E6C9"); // Darker green
                exerciseBorders[completedExercise].Stroke = Color.FromArgb("#4CAF50"); // Green border
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private async void OnCompleteWorkout_Clicked(object sender, EventArgs e)
    {
        // navigate to the new post-activity journal page
        await Shell.Current.GoToAsync(nameof(PostActivityJournalEntryPage));
    }
}
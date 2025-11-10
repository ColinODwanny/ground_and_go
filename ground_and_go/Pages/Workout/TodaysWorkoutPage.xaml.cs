//Aidan Trusky
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using ground_and_go.Services;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class TodaysWorkoutPage : ContentPage
{
    private Dictionary<string, Border> exerciseBorders;

    // a field for the progress service
    private readonly DailyProgressService _progressService;

    public TodaysWorkoutPage(DailyProgressService progressService)
    {
        InitializeComponent();
        
        //  Assign the service
        _progressService = progressService;
        
        // Initialize the dictionary to map exercise names to their buttons
          exerciseBorders = new Dictionary<string, Border>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Read the flow type directly from the service
        // We only check for "workout" because the "rest" flow
        // never comes to this page.
        if (_progressService.CurrentFlowType == "workout")
        {
            // WORKOUT FLOW (5 steps total)
            // Step 3 (Mindfulness) is done. This is Step 4.
            this.Title = "Step 4 of 5: Your Workout";
            ProgressStepLabel.Text = "Step 4 of 5: Complete your exercises";
            FlowProgressBar.Progress = 0.60; // 3/5 complete
        }

        // This is your existing code
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
                exerciseBorders[completedExercise].BackgroundColor = Color.FromArgb("#C8E6C9");
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
        // The service already knows we are in the "workout" flow.
        await Shell.Current.GoToAsync($"{nameof(PostActivityJournalEntryPage)}");
    }
}
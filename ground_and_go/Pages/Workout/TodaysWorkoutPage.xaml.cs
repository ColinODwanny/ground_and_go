//Aidan Trusky
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace ground_and_go.Pages.WorkoutGeneration;

// NEW: Add this QueryProperty
[QueryProperty(nameof(FlowType), "flow")]
public partial class TodaysWorkoutPage : ContentPage
{
    private Dictionary<string, Border> exerciseBorders;

    // NEW: Add this property
    public string? FlowType { get; set; }

    public TodaysWorkoutPage()
    {
        InitializeComponent();
        
        // Initialize the dictionary to map exercise names to their buttons
  	    exerciseBorders = new Dictionary<string, Border>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // NEW: Add this logic
        // We only check for "workout" because the "rest" flow
        // never comes to this page.
        if (FlowType == "workout")
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
        // UPDATED: Pass the flow parameter to the final page
        await Shell.Current.GoToAsync($"{nameof(PostActivityJournalEntryPage)}?flow=workout");
    }
}
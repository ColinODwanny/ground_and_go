//Devlin Delegard
// FILE: ground_and_go/Pages/Workout/RestDayPage.xaml.cs
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.Home;
using ground_and_go.Services;
using ground_and_go.Models;
using ground_and_go;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class MindfulnessActivityWorkoutPage : ContentPage
{

    // Add fields for our injected services ***
    private readonly DailyProgressService _progressService;
    private readonly Database _database;
    private readonly MockAuthService _authService;

    //  Update constructor to receive our services
    public MindfulnessActivityWorkoutPage(Database database, MockAuthService authService, DailyProgressService progressService)
    {
        InitializeComponent();
        _database = database;
        _authService = authService;
        _progressService = progressService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // WORKOUT FLOW (5 steps total)
        // Step 2 (Journal) is done. This is Step 3.
        // We don't need to check the flow type, because this page
        // is *only* for the workout flow.
        this.Title = "Step 3 of 5: Mindfulness";
        ProgressStepLabel.Text = "Step 3 of 5: Complete this activity";
        FlowProgressBar.Progress = 0.40; // 2/5 complete
    }

    // This method now passes the flow parameter
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // This button will eventually save the entry and navigate
        var popup = new WorkoutOptionsPopup();
        var result = await this.ShowPopupAsync(popup);
        
        
        // 1. Get the Log ID we saved in the previous step
        string? logId = _progressService.CurrentLogId;

        // 2. Get a workout ID (using a placeholder for now)
        int generatedWorkoutId = 201; 

        // 3. Save the workout ID to the log
        if (!string.IsNullOrEmpty(logId))
        {
            await _database.UpdateWorkoutIdAsync(logId, generatedWorkoutId);
        }
        else
        {
            // This should not happen, but it's good to check
            Console.WriteLine("Error: Could not find the CurrentLogId to save the workout_id.");
            await DisplayAlert("Error", "A problem occurred. Could not find the current log.", "OK");
            return;
        }

        // Navigate hierarchically using the alias "TheWorkout"
        await Shell.Current.GoToAsync("TheWorkout"); 
    }
}
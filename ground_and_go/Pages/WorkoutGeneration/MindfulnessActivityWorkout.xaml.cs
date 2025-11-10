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
        
        // 1. Get User ID and Today's Log
        int memberId = _authService.GetCurrentMemberId();
        WorkoutLog? todaysLog = await _database.GetTodaysWorkoutLog(memberId);

        // 2. Get a workout ID (using a placeholder for now)
        // TODO: Replace '1' with your actual workout generation logic
        int generatedWorkoutId = 1; 

        // 3. Save the workout ID to the log
        if (todaysLog != null)
        {
            await _database.UpdateWorkoutIdAsync(todaysLog.LogId, generatedWorkoutId);
        }
        else
        {
            // This should not happen, but it's good to check
            Console.WriteLine("Error: Could not find today's log to save workout_id.");
        }

        // after the popup closes, navigate to the main "workout" tab
        // the "//" means go to this absolute route
        // Navigate without the query parameter
        await Shell.Current.GoToAsync($"//workout"); 
    }
}
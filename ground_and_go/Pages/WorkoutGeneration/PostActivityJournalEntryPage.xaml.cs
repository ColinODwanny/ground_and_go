// Samuel Reynebeau
using ground_and_go.Models;
using ground_and_go.Services;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class PostActivityJournalEntryPage : ContentPage
{
    
    private readonly Database _database;
    private readonly MockAuthService _authService;
    // progress service
    private readonly DailyProgressService _progressService;

    // Update constructor to receive all services
    public PostActivityJournalEntryPage(Database database, MockAuthService authService, DailyProgressService progressService)
    {
        InitializeComponent();
        _database = database;
        _authService = authService;
        _progressService = progressService; // Assign the service
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Read the flow type directly from the service
        if (_progressService.CurrentFlowType == "workout")
        {
            // WORKOUT FLOW (5 steps total)
            // Step 4 (Workout) is done. This is Step 5.
            this.Title = "Step 5 of 5: Final Reflection";
            ProgressStepLabel.Text = "Step 5 of 5: Write a final reflection";
            FlowProgressBar.Progress = 0.80; // 4/5 complete
        }
        else // "rest" flow
        {
            // REST FLOW (4 steps total)
            // Step 3 (Mindfulness) is done. This is Step 4.
            this.Title = "Step 4 of 4: Final Reflection";
            ProgressStepLabel.Text = "Step 4 of 4: Write a final reflection";
            FlowProgressBar.Progress = 0.75; // 3/4 complete
        }

        // We can pre-load today's log to make saving faster,
        // but it's safer to get it fresh on the 'Finish' click
        // in case of any issues.
    }

    // This method now saves the journal entry
    private async void OnFinish_Clicked(object sender, EventArgs e)
    {
        try
        {
            // 1. Get the real user ID
            string? memberId = _database.GetAuthenticatedMemberId();

            if (string.IsNullOrEmpty(memberId))
            {
                await DisplayAlert("Error", "You are not logged in. Please restart the app.", "OK");
                return;
            }

            // 2. Get today's log from the database
            WorkoutLog? todaysLog = await _database.GetTodaysWorkoutLog(memberId);

            if (todaysLog != null)
            {
                // 3. Save the final journal text to that log
                await _database.UpdateAfterJournalAsync(todaysLog.LogId, JournalEditor.Text);
            }
            else
            {
                // This shouldn't happen, but just in case...
                Console.WriteLine("Error: Could not find today's log to save the after_journal.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving final journal: {ex.Message}");
            // Don't block the user, let them go home anyway
        }

        // 4. Navigate back to the main home tab
        await Shell.Current.GoToAsync("//home");
    }
}
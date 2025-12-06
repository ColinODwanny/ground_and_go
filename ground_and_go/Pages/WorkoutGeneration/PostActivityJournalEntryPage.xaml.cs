// FILE: ground_and_go/Pages/WorkoutGeneration/PostActivityJournalEntryPage.xaml.cs
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

        // Override UI Back Button (Top Left)
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            Command = new Command(async () => await NavigateBackToPreviousStep())
        });
    }

    // Override Hardware Back Button (Android)
    protected override bool OnBackButtonPressed()
    {
        // Use discard (_) to suppress the async warning
        _ = NavigateBackToPreviousStep();
        return true; // We handled the navigation manually
    }

    private async Task NavigateBackToPreviousStep()
    {
        string flow = _progressService.CurrentFlowType;
        
        if (flow == "workout")
        {
            // Workout Flow: Always go back to Workout (Step 4)
            await Shell.Current.GoToAsync($"//home/TheWorkout");
        }
        else // Rest Flow
        {
            // Rest Flow: Always go back to Mindfulness (Step 3)
            // (Since we forced mindfulness to be included for Rest Days)
            await Shell.Current.GoToAsync($"//home/{nameof(MindfulnessActivityRestPage)}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Use dynamic step counting based on emotion type from database
        // This is actual step 5 for the post-activity journal
        var (displayStep, totalSteps) = _progressService.GetDisplayStep(5);
        double progress = _progressService.GetProgressPercentage(5);
        
        this.Title = $"Step {displayStep} of {totalSteps}: Final Reflection";
        ProgressStepLabel.Text = $"Step {displayStep} of {totalSteps}: Write a final reflection";
        FlowProgressBar.Progress = progress;
    }

    // This method now saves the journal entry
    private async void OnFinish_Clicked(object sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("DEBUG: PostActivityJournalEntryPage - OnFinish_Clicked started");
            
            // 1. Get the Log ID we saved from the service
            string? logId = _progressService.CurrentLogId;
            Console.WriteLine($"DEBUG: CurrentLogId from service: '{logId ?? "NULL"}'");

            // 2. If CurrentLogId is null, try to get today's log directly (Recovery Logic)
            if (string.IsNullOrEmpty(logId))
            {
                Console.WriteLine("DEBUG: CurrentLogId is null, trying to get today's log directly");
                string? memberId = _database.GetAuthenticatedMemberId();
                if (!string.IsNullOrEmpty(memberId))
                {
                    var todaysLog = await _database.GetTodaysWorkoutLog(memberId);
                    if (todaysLog != null)
                    {
                        logId = todaysLog.LogId;
                        _progressService.CurrentLogId = logId;
                        Console.WriteLine($"DEBUG: Found today's log ID: '{logId}'");
                    }
                    else
                    {
                        Console.WriteLine("DEBUG: No today's log found!");
                    }
                }
                else
                {
                    Console.WriteLine("DEBUG: Member ID is null - user not logged in?");
                }
            }

            if (!string.IsNullOrEmpty(logId))
            {
                Console.WriteLine($"DEBUG: Attempting to save after_journal to log ID: '{logId}'");
                Console.WriteLine($"DEBUG: Journal text length: {JournalEditor.Text?.Length ?? 0} characters");
                
                // 3. Save the final journal text to that log
                await _database.UpdateAfterJournalAsync(logId, JournalEditor.Text ?? "");
                Console.WriteLine("DEBUG: UpdateAfterJournalAsync completed successfully");

                // 4. Clear the ID now that we're done
                _progressService.CurrentLogId = null;
                Console.WriteLine("DEBUG: Cleared CurrentLogId from service");
            }
            else
            {
                Console.WriteLine("ERROR: Could not find CurrentLogId to save the after_journal. This will prevent completion detection!");
                
                // Show an alert to the user about the issue
                await DisplayAlert("Warning", 
                    "There was an issue saving your completion status. Your workout may not show as complete. Please contact support if this persists.", 
                    "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Exception in OnFinish_Clicked: {ex.Message}");
            Console.WriteLine($"ERROR: Stack trace: {ex.StackTrace}");
            
            // Show error to user but don't block navigation
            await DisplayAlert("Error", 
                $"There was an error saving your journal entry: {ex.Message}. Please try again.", 
                "OK");
        }

        Console.WriteLine("DEBUG: Navigating back to home page");
        // 5. Navigate back to the main home tab
        await Shell.Current.GoToAsync("//home");
    }
}
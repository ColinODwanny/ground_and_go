// FILE: ground_and_go/Pages/WorkoutGeneration/JournalEntryPage.xaml.cs
// Samuel Reynebeau
using System.Text.Json;
using ground_and_go.Services;
using ground_and_go.Models;
using CommunityToolkit.Maui.Views;
using ground_and_go.Components;

namespace ground_and_go.Pages.WorkoutGeneration;

// add this attribute to tell the page the results of the popup
[QueryProperty(nameof(ResultJSON), "results")]
public partial class JournalEntryPage : ContentPage
{
    // Add private fields for our services
    private readonly Database _database;
    private readonly MockAuthService _authService;
    // Add a field for the progress service
    private readonly DailyProgressService _progressService;

    public FeelingResult? ResultType { get; set; }
    private string? _resultJSON;
    public string? ResultJSON
    {
        get => _resultJSON;
        set
        {
            _resultJSON = value;
            ResultType = JsonSerializer.Deserialize<FeelingResult>(_resultJSON!); //Converts the JSON into the feelings inputted by the user
        }
    }

    public JournalEntryPage(Database database, MockAuthService authService, DailyProgressService progressService)
    {
        InitializeComponent();

        _database = database;
        _authService = authService;
        // Assign the new service ***
        _progressService = progressService;

        // Override the Top-Left UI Back Button
        // If the user hits back here, they want to change their emotion.
        // We must cancel the current flow (delete the log) so they aren't trapped in "Resume".
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            Command = new Command(async () => await CancelAndReturnHome())
        });
    }

    // Override the Hardware Back Button (Android)
    protected override bool OnBackButtonPressed()
    {
        _ = CancelAndReturnHome();
        return true; 
    }

    private async Task CancelAndReturnHome()
    {
        // 1. Get the current Log ID (created in Step 1)
        string? logId = _progressService.CurrentLogId;

        // 2. Delete it from the database
        // This removes the "Resume" state, allowing the user to start fresh from Home.
        // This ONLY happens if they click Back. If they Logout/Crash, this doesn't run, so Resume remains safe.
        if (!string.IsNullOrEmpty(logId))
        {
            await _database.DeleteWorkoutLog(logId);
        }

        // 3. Clear local service state
        _progressService.ResetDailyState();

        // 4. Go back to Home
        await Shell.Current.GoToAsync("//home");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Use dynamic step counting based on emotion type from database
        // Journal is actual step 2
        var (displayStep, totalSteps) = await _progressService.GetDisplayStepAsync(2);
        double progress = await _progressService.GetProgressPercentageAsync(2);

        this.Title = $"Step {displayStep} of {totalSteps}: Journal";
        ProgressStepLabel.Text = $"Step {displayStep} of {totalSteps}: Write a reflection";
        FlowProgressBar.Progress = progress;
    }

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // 1. Get the real user ID from the database service
        string? memberId = _database.GetAuthenticatedMemberId();

        // 1a. check if user is actually logged in
        if (string.IsNullOrEmpty(memberId))
        {
            await DisplayAlert("Error", "You are not logged in. Please restart the app.", "OK");
            return;
        }

        // 2. Save the journal entry to the database
        // We check the service first to see if a log was already started (Step 1)
        string? logId = _progressService.CurrentLogId;

        if (!string.IsNullOrEmpty(logId))
        {
            // Update the existing log (replacing the "STATE:..." placeholder)
            await _database.UpdateBeforeJournal(logId, JournalEntry.Text);
        }
        else
        {
            // Fallback: Create initial log if for some reason it doesn't exist
            var newLog = await _database.CreateInitialWorkoutLog(memberId, JournalEntry.Text);
            if (newLog != null)
            {
                // 3. Save the new LogId in our service
                _progressService.CurrentLogId = newLog.LogId;
                logId = newLog.LogId; // Update local variable for use below
            }
            else
            {
                // The insert failed, so we should stop here
                await DisplayAlert("Error", "There was a problem saving your journal entry. Please try again.", "OK");
                return;
            }
        }

        // 4. Navigate to the next page
        // Read the flow type directly from the service
        string currentFlow = _progressService.CurrentFlowType ?? "workout";
        
        // Determine if we need mindfulness based on flow and emotion
        bool requiresMindfulness = await _progressService.RequiresMindfulnessAsync();

        if (currentFlow == "workout")
        {
            if (requiresMindfulness)
            {
                // Emotions with workout mindfulness activities - go through mindfulness
                await Shell.Current.GoToAsync($"{nameof(MindfulnessActivityWorkoutPage)}");
            }
            else
            {
                // Emotions without workout mindfulness (Happy/Energized) - skip mindfulness
                // Go directly to workout selection/generation
                await NavigateDirectlyToWorkout();
            }
        }
        else if (currentFlow == "rest")
        {
            
            await Shell.Current.GoToAsync($"{nameof(MindfulnessActivityRestPage)}");
        }
        else
        {
            // just in case
            await DisplayAlert("Error", "Could not determine navigation flow.", "OK");
        }
    }

    private async Task NavigateDirectlyToWorkout()
    {
        string userEmotion = _progressService.CurrentFeelingResult?.Mood ?? "";

        // Get available workout categories for this emotion
        var availableCategories = await _database.GetWorkoutCategoriesByEmotion(userEmotion);

        if (availableCategories.Count == 0)
        {
            await DisplayAlert("No Workouts Available",
                $"No workouts are available for the emotion '{userEmotion}'. Please try a different emotion.",
                "OK");
            return;
        }

        EquipmentResult? equipmentResult = null;

        if (availableCategories.Count == 1)
        {
            // Only one workout type available - auto-generate without popup
            string singleWorkoutType = availableCategories[0];

            // Check if equipment selection is needed
            bool needsEquipment = await _database.IsEquipmentSelectionNeeded(userEmotion, singleWorkoutType);

            if (needsEquipment)
            {
                // Still need to ask about equipment for this single category
                var popup = new WorkoutOptionsPopup(availableCategories, userEmotion, _database);
                equipmentResult = await this.ShowPopupAsync(popup) as EquipmentResult;
            }
            else
            {
                // Auto-generate with no equipment preference
                equipmentResult = new EquipmentResult
                {
                    WorkoutType = singleWorkoutType,
                    GymAccess = false,
                    HomeAccess = false
                };
            }
        }
        else
        {
            // Multiple workout types available - show selection popup
            var popup = new WorkoutOptionsPopup(availableCategories, userEmotion, _database);
            equipmentResult = await this.ShowPopupAsync(popup) as EquipmentResult;
        }

        if (equipmentResult is not EquipmentResult equipment)
        {
            return; // User cancelled equipment selection
        }

        // Store equipment result in service for later use
        _progressService.CurrentEquipmentResult = equipment;

        // Generate workout based on user's emotion and equipment
        Models.Workout? selectedWorkout = null;

        if (_progressService.CurrentFeelingResult?.Mood != null)
        {
            bool hasGymAccess = equipment.GymAccess;
            string workoutType = equipment.WorkoutType;

            selectedWorkout = await _database.GetRandomWorkoutByExactCriteria(userEmotion, hasGymAccess, workoutType);
        }

        if (selectedWorkout == null)
        {
            string equipmentText = equipment.GymAccess ? "gym equipment" : "home equipment";
            await DisplayAlert("No Workouts Available",
                $"No workouts are available for {userEmotion} mood with {equipment.WorkoutType} using {equipmentText}.\n\nPlease go back and try:\n• A different workout type\n• Different equipment preference\n• Or select a different emotion",
                "OK");
            return;
        }

        // Store the selected workout in the progress service
        _progressService.CurrentWorkout = selectedWorkout;

        // Save the workout to the log
        string? logId = _progressService.CurrentLogId;
        if (logId != null && selectedWorkout != null)
        {
            await _database.UpdateWorkoutIdAsync(logId, selectedWorkout.WorkoutId);
        }

        // Navigate directly to the workout
        await Shell.Current.GoToAsync("TheWorkout");
    }
}
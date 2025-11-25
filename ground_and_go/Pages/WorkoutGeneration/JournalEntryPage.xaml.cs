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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Use dynamic step counting based on emotion type from database
        var (displayStep, totalSteps) = await _progressService.GetDisplayStepAsync(2); // Journal is actual step 2
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
        
        // 1b. Check if user has already completed activity today
        var existingLog = await _database.GetTodaysWorkoutLog(memberId);
        if (existingLog != null && !string.IsNullOrEmpty(existingLog.AfterJournal))
        {
            await DisplayAlert("Already Complete", 
                "You've already completed your daily activity! Come back tomorrow for a new activity.", 
                "OK");
            await Shell.Current.GoToAsync("//HomePage");
            return;
        }
        
        // 2. Save the initial journal entry to the database
        // This creates the log for today and sets our database progress
        WorkoutLog? newLog = await _database.CreateInitialWorkoutLog(memberId, JournalEntry.Text);

        // 3. Save the new LogId in our service
        if (newLog != null)
        {
            _progressService.CurrentLogId = newLog.LogId;
        }
        else
        {
            // The insert failed, so we should stop here
            await DisplayAlert("Error", "There was a problem saving your journal entry. Please try again.", "OK");
            return;
        }
        
        // 4. Navigate to the next page
        // Read the flow type directly from the service
        if (_progressService.CurrentFlowType == "workout")
        {
            // Check if emotion should get mindfulness in workout flow (excludes Happy/Energized)
            string userEmotion = _progressService.CurrentFeelingResult?.Mood ?? "";
            bool hasMindfulnessActivities = await _database.HasWorkoutMindfulnessActivitiesForEmotion(userEmotion);
            
            if (hasMindfulnessActivities)
            {
                // Emotions with workout mindfulness activities - go through mindfulness
                Console.WriteLine($"DEBUG: WORKOUT FLOW - '{userEmotion}' should get mindfulness, navigating to mindfulness page");
                await Shell.Current.GoToAsync($"{nameof(MindfulnessActivityWorkoutPage)}");
            }
            else
            {
                // Emotions without workout mindfulness (Happy/Energized) - skip mindfulness
                Console.WriteLine($"DEBUG: WORKOUT FLOW - '{userEmotion}' should skip mindfulness, going to workout selection");
                await NavigateDirectlyToWorkout();
            }
        }
        else if (_progressService.CurrentFlowType == "rest")
        {
            // Check if emotion has mindfulness activities available in database (same logic as workout flow)
            string userEmotion = _progressService.CurrentFeelingResult?.Mood ?? "";
            bool hasMindfulnessActivities = await _database.HasMindfulnessActivitiesForEmotion(userEmotion);
            
            if (hasMindfulnessActivities)
            {
                // Emotions with mindfulness activities - go through mindfulness
                Console.WriteLine($"DEBUG: REST DAY - '{userEmotion}' has mindfulness activities available, navigating to mindfulness page");
                await Shell.Current.GoToAsync($"{nameof(MindfulnessActivityRestPage)}");
            }
            else
            {
                // Emotions without mindfulness activities - skip mindfulness (this should be rare for rest day)
                Console.WriteLine($"DEBUG: REST DAY - '{userEmotion}' has no mindfulness activities, skipping to post-journal");
                await Shell.Current.GoToAsync("PostJournal");
            }
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
                
                Console.WriteLine($"DEBUG: Auto-generating {singleWorkoutType} workout for {userEmotion} (no equipment needed)");
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
            
            Console.WriteLine($"DEBUG: Generating workout for emotion: '{userEmotion}', gym access: {hasGymAccess}, workout type: '{workoutType}'");
            Console.WriteLine($"DEBUG: EquipmentResult - GymAccess: {equipment.GymAccess}, HomeAccess: {equipment.HomeAccess}, WorkoutType: {workoutType}");
            
            selectedWorkout = await _database.GetRandomWorkoutByExactCriteria(userEmotion, hasGymAccess, workoutType);
            Console.WriteLine($"DEBUG: Selected workout result: {selectedWorkout?.WorkoutId} (Category: {selectedWorkout?.Category})");
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
            Console.WriteLine($"DEBUG: Saved workout ID {selectedWorkout.WorkoutId} to log {logId}");
        }

        // Navigate directly to the workout
        await Shell.Current.GoToAsync("TheWorkout");
    }
}
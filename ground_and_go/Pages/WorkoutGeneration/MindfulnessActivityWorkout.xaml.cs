//Devlin Delegard
// FILE: ground_and_go/Pages/Workout/RestDayPage.xaml.cs
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.Home;
using ground_and_go.Services;
using ground_and_go.Models;
using ground_and_go;
using ground_and_go.Pages.WorkoutGeneration;
using ground_and_go.enums;
using ground_and_go.Components;
using System.Threading.Tasks;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class MindfulnessActivityWorkoutPage : ContentPage
{

    // Add fields for our injected services ***
    private readonly DailyProgressService _progressService;
    private readonly Database _database;
    private readonly MockAuthService _authService;

    private MindfulnessActivity _activity;

    //  Update constructor to receive our services
    public MindfulnessActivityWorkoutPage(Database database, MockAuthService authService, DailyProgressService progressService)
    {
        InitializeComponent();
        _database = database;
        _authService = authService;
        _progressService = progressService;
        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Use dynamic step counting based on emotion type from database
        var (displayStep, totalSteps) = await _progressService.GetDisplayStepAsync(3); // Mindfulness is actual step 3
        double progress = await _progressService.GetProgressPercentageAsync(3);
        
        this.Title = $"Step {displayStep} of {totalSteps}: Mindfulness";
        ProgressStepLabel.Text = $"Step {displayStep} of {totalSteps}: Complete this activity";
        FlowProgressBar.Progress = progress;
        Enum.TryParse<Emotion>(
            _progressService.CurrentFeelingResult.Mood,
            ignoreCase: true,
            out var emotion
        );
        _activity = await _database.GetMindfulnessActivityByEmotion(emotion);
    }

    // This method now passes the flow parameter
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        string userEmotion = _progressService.CurrentFeelingResult?.Mood ?? "";
        
        // Note: Bad emotions go through mindfulness first, then proceed to workout selection

        // Get available workout categories for this emotion
        var availableCategories = await _database.GetWorkoutCategoriesByEmotion(userEmotion);
        
        if (availableCategories.Count == 0)
        {
            await DisplayAlert("No Workouts Available", 
                $"No workouts are available for the emotion '{userEmotion}'. Please try a mindfulness activity instead.", 
                "OK");
            await Shell.Current.GoToAsync("//HomePage");
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
        
        // 1. Get the Log ID we saved in the previous step
        string? logId = _progressService.CurrentLogId;

        // 2. Generate workout based on user's emotion and equipment
        Models.Workout? selectedWorkout = null;
        
        if (_progressService.CurrentFeelingResult?.Mood != null)
        {
            bool hasGymAccess = equipment.GymAccess;
            string workoutType = equipment.WorkoutType;
            
            Console.WriteLine($"DEBUG: Generating workout for emotion: '{userEmotion}', gym access: {hasGymAccess}, workout type: '{workoutType}'");
            Console.WriteLine($"DEBUG: EquipmentResult - GymAccess: {equipment.GymAccess}, HomeAccess: {equipment.HomeAccess}, WorkoutType: {workoutType}");
            
            Console.WriteLine($"DEBUG: About to query for: Emotion='{userEmotion}', Gym={hasGymAccess}, Type='{workoutType}'");
            
            selectedWorkout = await _database.GetRandomWorkoutByExactCriteria(userEmotion, hasGymAccess, workoutType);
            Console.WriteLine($"DEBUG: Selected workout result: {selectedWorkout?.WorkoutId} (Category: {selectedWorkout?.Category})");
            
        }
        else
        {
            Console.WriteLine($"DEBUG: No feeling result available - CurrentFeelingResult: {_progressService.CurrentFeelingResult}, Mood: {_progressService.CurrentFeelingResult?.Mood}");
        }

        if (selectedWorkout == null)
        {
            string equipmentText = equipment?.GymAccess == true ? "gym equipment" : "home equipment";
            string workoutType = equipment?.WorkoutType ?? "unknown type";
            await DisplayAlert("No Workouts Available", 
                $"No workouts are available for {userEmotion} mood with {workoutType} using {equipmentText}.\n\nPlease go back and try:\n• A different workout type\n• Different equipment preference\n• Or try doing mindfulness activities instead", 
                "OK");
            return;
        }

        // 3. Store the selected workout in the service for the workout page to access
        _progressService.CurrentWorkout = selectedWorkout;
        Console.WriteLine($"DEBUG: Stored workout {selectedWorkout.WorkoutId} in progress service");
        
        // 4. Save the workout ID to the log
        if (!string.IsNullOrEmpty(logId))
        {
            await _database.UpdateWorkoutIdAsync(logId, selectedWorkout.WorkoutId);
            Console.WriteLine($"DEBUG: Saved workout ID {selectedWorkout.WorkoutId} to log {logId}");
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

    private async void OnOpenYoutubeClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(_activity.YoutubeLink))
        {
            // CHANGED HERE: External → SystemPreferred (works on iPhone + Android)
            await Browser.OpenAsync(_activity.YoutubeLink, BrowserLaunchMode.SystemPreferred);
        }
    }

}

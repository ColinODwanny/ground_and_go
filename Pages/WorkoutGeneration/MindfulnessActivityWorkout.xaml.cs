// FILE: ground_and_go/Pages/WorkoutGeneration/MindfulnessActivityWorkout.xaml.cs
// Devlin Delegard
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.Home;
using ground_and_go.Services;
using ground_and_go.Models;
using ground_and_go.enums;
using ground_and_go.Components;
using System.Text.Json; // Needed for JSON

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class MindfulnessActivityWorkoutPage : ContentPage
{

    // Add fields for our injected services ***
    private readonly DailyProgressService _progressService;
    private readonly Database _database;
    private readonly MockAuthService _authService;

    private MindfulnessActivity? _activity;

    public MindfulnessActivityWorkoutPage(Database database, MockAuthService authService, DailyProgressService progressService)
    {
        InitializeComponent();
        _database = database;
        _authService = authService;
        _progressService = progressService;

        // UI Back Button Override
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            Command = new Command(async () => await NavigateBackToJournal())
        });
    }

    // Hardware Back Button Override
    protected override bool OnBackButtonPressed()
    {
        // Use discard (_) to suppress CS4014 warning for fire-and-forget async call
        _ = NavigateBackToJournal();
        return true; 
    }

    private async Task NavigateBackToJournal()
    {
        var result = _progressService.CurrentFeelingResult;
        if (result == null) result = new FeelingResult { Mood = "Neutral", Rating = 5 };
        
        var json = JsonSerializer.Serialize(result);
        
        // Navigate "Back" to Step 2
        await Shell.Current.GoToAsync($"//home/WorkoutJournalEntry?flow=workout&results={json}");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        var (displayStep, totalSteps) = _progressService.GetDisplayStep(3); 
        double progress = _progressService.GetProgressPercentage(3);
        
        this.Title = $"Step {displayStep} of {totalSteps}: Mindfulness";
        ProgressStepLabel.Text = $"Step {displayStep} of {totalSteps}: Complete this activity";
        FlowProgressBar.Progress = progress;
        
        Enum.TryParse<Emotion>(
            _progressService.CurrentFeelingResult?.Mood,
            true,
            out var emotion
        );
        if(_activity == null){
            // This assignment is now safe because _activity is nullable (?)
            _activity = await _database.GetMindfulnessActivityByEmotion(emotion);
        }
        if (_activity != null)
        {
            ActivityNameLabel.Text = _activity.ActivityName;
            ActivityDescriptionLabel.Text = "Most Activities are Best Experienced with Headphones.";
            YouTubeBtn.Text = "Start Activity";
        }
    }

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        string userEmotion = _progressService.CurrentFeelingResult?.Mood ?? "";

        var availableCategories = await _database.GetWorkoutCategoriesByEmotion(userEmotion);
        
        if (availableCategories.Count == 0)
        {
            await DisplayAlert("No Workouts", "No workouts available.", "OK");
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
                equipmentResult = new EquipmentResult { WorkoutType = singleWorkoutType, GymAccess = false, HomeAccess = false };
            }
        }
        else
        {
            // Multiple workout types available - show selection popup
            var popup = new WorkoutOptionsPopup(availableCategories, userEmotion, _database);
            equipmentResult = await this.ShowPopupAsync(popup) as EquipmentResult;
        }
        
        if (equipmentResult is not EquipmentResult equipment) return;

        _progressService.CurrentEquipmentResult = equipment;
        
        // 1. Get the Log ID we saved in the previous step
        string? logId = _progressService.CurrentLogId;

        // 2. Generate workout based on user's emotion and equipment
        Models.Workout? selectedWorkout = null;
        if (_progressService.CurrentFeelingResult?.Mood != null)
        {
            selectedWorkout = await _database.GetRandomWorkoutByExactCriteria(userEmotion, equipment.GymAccess, equipment.WorkoutType);
        }

        if (selectedWorkout == null)
        {
            await DisplayAlert("No Workouts", "No workouts found.", "OK");
            return;
        }

        _progressService.CurrentWorkout = selectedWorkout;
        
        if (!string.IsNullOrEmpty(logId))
        {
            await _database.UpdateWorkoutIdAsync(logId, selectedWorkout.WorkoutId);
        }

        await Shell.Current.GoToAsync("TheWorkout"); 
    }

    private async void OnOpenYoutubeClicked(object sender, EventArgs e)
    {
        if (_activity != null && !string.IsNullOrEmpty(_activity.YoutubeLink))
        {
            await Browser.OpenAsync(_activity.YoutubeLink, BrowserLaunchMode.SystemPreferred);
        }
    }
}
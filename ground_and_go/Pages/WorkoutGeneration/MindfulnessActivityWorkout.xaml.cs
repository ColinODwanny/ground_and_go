//Devlin Delegard
// FILE: ground_and_go/Pages/Workout/RestDayPage.xaml.cs
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.Home;
using ground_and_go.Services;
using ground_and_go.Models;
using ground_and_go;
using ground_and_go.Models;
using ground_and_go.Pages.WorkoutGeneration;
using ground_and_go.enums;
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
        
        // WORKOUT FLOW (5 steps total)
        // Step 2 (Journal) is done. This is Step 3.
        // We don't need to check the flow type, because this page
        // is *only* for the workout flow.
        this.Title = "Step 3 of 5: Mindfulness";
        ProgressStepLabel.Text = "Step 3 of 5: Complete this activity";
        FlowProgressBar.Progress = 0.40; // 2/5 complete
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
        // Show equipment selection popup
        var popup = new WorkoutOptionsPopup();
        var equipmentResult = await this.ShowPopupAsync(popup);
        
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
            string userEmotion = _progressService.CurrentFeelingResult.Mood;
            string workoutType = equipment.WorkoutType;
            
            Console.WriteLine($"DEBUG: Generating workout for emotion: '{userEmotion}', gym access: {hasGymAccess}, workout type: '{workoutType}'");
            Console.WriteLine($"DEBUG: EquipmentResult - GymAccess: {equipment.GymAccess}, HomeAccess: {equipment.HomeAccess}, WorkoutType: {workoutType}");
            
            Console.WriteLine($"DEBUG: About to query for: Emotion='{userEmotion}', Gym={hasGymAccess}, Type='{workoutType}'");
            
            selectedWorkout = await _database.GetRandomWorkoutByEmotionAndEquipment(userEmotion, hasGymAccess, workoutType);
            
            Console.WriteLine($"DEBUG: Selected workout result: {selectedWorkout?.WorkoutId} (Category: {selectedWorkout?.Category})");
        }
        else
        {
            Console.WriteLine($"DEBUG: No feeling result available - CurrentFeelingResult: {_progressService.CurrentFeelingResult}, Mood: {_progressService.CurrentFeelingResult?.Mood}");
        }

        if (selectedWorkout == null)
        {
            await DisplayAlert("Error", "Could not generate a workout based on your preferences. Please try again.", "OK");
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
            await Browser.OpenAsync(_activity.YoutubeLink, BrowserLaunchMode.External);
        }
    }

}
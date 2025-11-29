// FILE: ground_and_go/Pages/WorkoutGeneration/MindfulnessActivityRest.xaml.cs
using ground_and_go.Pages.Home;
using ground_and_go.Services;
using ground_and_go.Models;
using ground_and_go.enums;
using System.Text.Json; // Needed for JSON

namespace ground_and_go.Pages.WorkoutGeneration;

[QueryProperty(nameof(FlowType), "flow")]
public partial class MindfulnessActivityRestPage : ContentPage
{
    public string? FlowType { get; set; }
    
    private readonly Database _database;
    private readonly DailyProgressService _progressService;
    private MindfulnessActivity? _activity;

    public MindfulnessActivityRestPage(Database database, DailyProgressService progressService)
    {
        InitializeComponent();
        _database = database;
        _progressService = progressService;

        // Override the Top-Left UI Back Button
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            Command = new Command(async () => await NavigateBackToJournal())
        });
    }

    // Override the Hardware Back Button (Android)
    protected override bool OnBackButtonPressed()
    {
        // Use discard (_) to fire-and-forget the async task
        _ = NavigateBackToJournal();
        
        return true; // We handled it
    }

    private async Task NavigateBackToJournal()
    {
        // Re-package the feeling result so the Journal Page can render the header
        var result = _progressService.CurrentFeelingResult;
        var json = JsonSerializer.Serialize(result);
        
        // Manually navigate "Back" to Step 2
        await Shell.Current.GoToAsync($"//home/WorkoutJournalEntry?flow=rest&results={json}");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Use dynamic step counting for rest day flow
        var (displayStep, totalSteps) = await _progressService.GetDisplayStepAsync(3);
        double progress = await _progressService.GetProgressPercentageAsync(3);
        
        this.Title = $"Step {displayStep} of {totalSteps}: Mindfulness";
        ProgressStepLabel.Text = $"Step {displayStep} of {totalSteps}: Complete this activity";
        FlowProgressBar.Progress = progress;
        
        // Load mindfulness activity from database
        Enum.TryParse<Emotion>(
            _progressService.CurrentFeelingResult?.Mood,
            true,
            out var emotion
        );
        _activity = await _database.GetMindfulnessActivityByEmotion(emotion);
        
        // Update UI with activity details if found
        if (_activity != null)
        {
            ActivityNameLabel.Text = _activity.ActivityName;
            ActivityDescriptionLabel.Text = "Most Activities are Best Experienced with Headphones.";
            YouTubeBtn.Text = "Start Activity";
        }
        else
        {
            ActivityNameLabel.Text = "Rest Day Mindfulness";
            ActivityDescriptionLabel.Text = "Take a moment to relax and reflect.";
            YouTubeBtn.Text = "Continue";
        }
    }

    private async void OnOpenYoutubeClicked(object sender, EventArgs e)
    {
        if (_activity != null && !string.IsNullOrEmpty(_activity.YoutubeLink))
        {
            try
            {
                await Launcher.OpenAsync(_activity.YoutubeLink);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening YouTube link: {ex.Message}");
                await DisplayAlert("Error", "Could not open the mindfulness video.", "OK");
            }
        }
    }

    // Save "STATE:Pending" before navigating
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        string? logId = _progressService.CurrentLogId;
        
        if (!string.IsNullOrEmpty(logId))
        {
            // Update DB to say: "Middle activity done, waiting for Post-Journal"
            await _database.UpdateAfterJournalAsync(logId, "STATE:Pending");
        }

        await Shell.Current.GoToAsync("PostJournal");
    }
}
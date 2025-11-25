//Devlin Delegard
// FILE: ground_and_go/Pages/Workout/RestDayPage.xaml.cs
using ground_and_go.Pages.Home;
using ground_and_go.Services;
using ground_and_go.Models;
using ground_and_go.enums;

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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // REST DAY always has 4 steps: emotion → journal → mindfulness → post-journal (no workout)
        // This is step 3 of 4 for mindfulness
        this.Title = "Step 3 of 4: Mindfulness";
        ProgressStepLabel.Text = "Step 3 of 4: Complete this activity";
        FlowProgressBar.Progress = 0.50; // 2/4 complete
        
        // Load mindfulness activity from database
        Enum.TryParse<Emotion>(
            _progressService.CurrentFeelingResult?.Mood,
            ignoreCase: true,
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

    // This method opens the YouTube link for the mindfulness activity
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
                await DisplayAlert("Error", "Could not open the mindfulness video. Please continue with the rest of your activity.", "OK");
            }
        }
    }

    // This method passes the flow parameter
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // Navigate hierarchically using the alias "PostJournal"
        await Shell.Current.GoToAsync("PostJournal");
    }
}
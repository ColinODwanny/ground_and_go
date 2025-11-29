// FILE: ground_and_go/Pages/WorkoutGeneration/HowDoYouFeelPopup.xaml.cs
// Samuel Reynebeau
using CommunityToolkit.Maui.Views;
using System.Linq;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class HowDoYouFeelPopup : Popup
{
    private Button? _selectedMoodButton;
    private string _flowType;
    public static FeelingResult? feelingResult;
    
    public HowDoYouFeelPopup(string flowType = "workout")
    {
        InitializeComponent();
        _flowType = flowType;
        
        // Set initial step display
        ProgressStepLabel.Text = "Step 1: Choose your emotion";
        FlowProgressBar.Progress = 0.0;
    }

    private void OnCancel_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    private async void OnSubmit_Clicked(object sender, EventArgs e)
    {
        if (_selectedMoodButton == null)
        {
            await Shell.Current.DisplayAlert("Missing mood", "Please select a mood before submitting.", "OK");
            return;
        }
        
        var result = new FeelingResult
        {
            Rating = RatingSlider.Value,
            Mood = _selectedMoodButton?.Text
        };

        feelingResult = result;
        Close(result);
    }

    private void OnMoodClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
        {
            if (_selectedMoodButton != null)
            {
                _selectedMoodButton.BackgroundColor = Color.FromArgb("#F3F4F6");
                _selectedMoodButton.TextColor = Colors.Black;
                _selectedMoodButton.BorderColor = Color.FromArgb("#E0E0E0");
            }

            _selectedMoodButton = clickedButton;
            _selectedMoodButton.BackgroundColor = Color.FromArgb("#2196F3");
            _selectedMoodButton.TextColor = Colors.White;
            _selectedMoodButton.BorderColor = Color.FromArgb("#2196F3");
            
            UpdateProgressDisplay(clickedButton.Text);
        }
    }
    
    private void UpdateProgressDisplay(string selectedMood)
    {
        int totalSteps;
        
        var emotionsSkippingMindfulness = new HashSet<string> { "Happy", "Energized" };
        bool isPositiveMood = emotionsSkippingMindfulness.Contains(selectedMood);

        if (_flowType == "rest")
        {
            // FIX: Rest Day ALWAYS includes mindfulness now, regardless of emotion.
            // Emotion(1) -> Journal(2) -> Mindfulness(3) -> Post-Journal(4)
            totalSteps = 4;
        }
        else
        {
            // Workout Flow Logic:
            // Positive = 4 steps (Skip Mind)
            // Negative = 5 steps (Do Mind)
            totalSteps = isPositiveMood ? 4 : 5;
        }
        
        ProgressStepLabel.Text = $"Step 1 of {totalSteps}: Choose your emotion";
        FlowProgressBar.Progress = 0.0;
    }
}
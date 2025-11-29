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

        // Update the static field just in case other parts of your app use it
        feelingResult = result;

        Close(result);
    }

    private void OnMoodClicked(object sender, EventArgs e)
    {
        if (sender is Button clickedButton)
        {
            // Deselect previous
            if (_selectedMoodButton != null)
            {
                _selectedMoodButton.BackgroundColor = Color.FromArgb("#F3F4F6");
                _selectedMoodButton.TextColor = Colors.Black;
                _selectedMoodButton.BorderColor = Color.FromArgb("#E0E0E0");
            }

            // Select new one
            _selectedMoodButton = clickedButton;
            _selectedMoodButton.BackgroundColor = Color.FromArgb("#2196F3");
            _selectedMoodButton.TextColor = Colors.White;
            _selectedMoodButton.BorderColor = Color.FromArgb("#2196F3");
            
            // Update progress display based on selected emotion
            UpdateProgressDisplay(clickedButton.Text);
        }
    }
    
    private void UpdateProgressDisplay(string selectedMood)
    {
        int totalSteps;
        
        var emotionsSkippingMindfulness = new HashSet<string> { "Happy", "Energized" };
        bool skipsMindfulness = emotionsSkippingMindfulness.Contains(selectedMood);

        if (_flowType == "rest")
        {
            // Rest Flow:
            // Standard: 4 Steps (Emotion -> Journal -> Mindfulness -> Post-Journal)
            // Happy/Energized: 3 Steps (Emotion -> Journal -> Post-Journal)
            totalSteps = skipsMindfulness ? 3 : 4;
        }
        else
        {
            // Workout Flow:
            // Standard: 5 Steps (Emotion -> Journal -> Mindfulness -> Workout -> Post-Journal)
            // Happy/Energized: 4 Steps (Emotion -> Journal -> Workout -> Post-Journal)
            totalSteps = skipsMindfulness ? 4 : 5;
        }
        
        ProgressStepLabel.Text = $"Step 1 of {totalSteps}: Choose your emotion";
        FlowProgressBar.Progress = 0.0;
        
        Console.WriteLine($"DEBUG: Updated progress display for '{selectedMood}' in {_flowType} flow - {totalSteps} total steps");
    }
}
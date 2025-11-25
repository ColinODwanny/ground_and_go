// Samuel Reynebeau
using CommunityToolkit.Maui.Views;
using System.Linq;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class HowDoYouFeelPopup : Popup
{
    private Button _selectedMoodButton;
    private string _flowType;
    public static FeelingResult feelingResult;
    
	public HowDoYouFeelPopup(string flowType = "workout")
	{
        InitializeComponent();
        _flowType = flowType;
        
        // Set initial step display (we don't know the emotion yet, so show generic)
        ProgressStepLabel.Text = "Step 1: Choose your emotion";
        FlowProgressBar.Progress = 0.0;
    }

    //close the window when cancel is clicked
    private void OnCancel_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    // submit
    private void OnSubmit_Clicked(object sender, EventArgs e)
    {

        if (_selectedMoodButton == null)
    {
        Application.Current.MainPage.DisplayAlert("Missing mood", "Please select a mood before submitting.", "OK");
        return;
    }
        var result = new FeelingResult
        {
            Rating = RatingSlider.Value,
            Mood = _selectedMoodButton?.Text
        };

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
    
    private async void UpdateProgressDisplay(string selectedMood)
    {
        int totalSteps;
        
        if (_flowType == "rest")
        {
            // Rest day flow always has 4 steps: emotion → journal → mindfulness → post-journal
            totalSteps = 4;
        }
        else
        {
            // Workout flow: Happy and Energized skip mindfulness (4 steps), others have mindfulness (5 steps)
            var emotionsWithoutMindfulnessInWorkout = new HashSet<string> { "Happy", "Energized" };
            bool skipsMindfulness = emotionsWithoutMindfulnessInWorkout.Contains(selectedMood);
            totalSteps = skipsMindfulness ? 4 : 5;
        }
        
        ProgressStepLabel.Text = $"Step 1 of {totalSteps}: Choose your emotion";
        // Still at 0 progress since this is the first step
        FlowProgressBar.Progress = 0.0;
        
        Console.WriteLine($"DEBUG: Updated progress display for '{selectedMood}' in {_flowType} flow - {totalSteps} total steps");
    }
}
// Samuel Reynebeau
using CommunityToolkit.Maui.Views;
using System.Linq;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class HowDoYouFeelPopup : Popup
{
    private Button _selectedMoodButton;
    public static FeelingResult feelingResult;
	public HowDoYouFeelPopup()
	{
        InitializeComponent();
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
        }
    }
}
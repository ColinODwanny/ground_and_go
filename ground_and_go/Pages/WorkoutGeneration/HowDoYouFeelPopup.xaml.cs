// Samuel Reynebeau
using CommunityToolkit.Maui.Views;
using System.Linq;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class HowDoYouFeelPopup : Popup
{
	public HowDoYouFeelPopup()
	{
        InitializeComponent();

        //initialize neutral button as the default radio button that's clicked
        var neutralButton = MoodFlexLayout.Children.FirstOrDefault(c => (c as RadioButton)?.Value as string == "Neutral");
        if(neutralButton is RadioButton rb)
        {
            rb.IsChecked = true;
        }
    }

    //close the window when cancel is clicked
    private void OnCancel_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    // submit
    private void OnSubmit_Clicked(object sender, EventArgs e)
    {
        var result = new FeelingResult
        {
            Rating = RatingSlider.Value,
            Mood = CalculateMood()
        };

        Close(result);
    }

    /// <summary>
    /// Takes all checked boxes and adds them to a string array
    /// </summary>
    /// <returns>A nullable string array containing checked emotions</returns>
    private String?[] CalculateMood()
    {
        List<String?> mood = []; //A list dynamically adds emotions to it
        if (Happy.IsChecked)
            mood.Add("Happy");
        if (Energized.IsChecked)
            mood.Add("Energized");
        if (Neutral.IsChecked)
            mood.Add("Neutral");
        if (Anxious.IsChecked)
            mood.Add("Anxious");
        if (Sad.IsChecked)
            mood.Add("Sad");
        if (Angry.IsChecked)
            mood.Add("Angry");
        if (Depressed.IsChecked)
            mood.Add("Depressed");
        if (Tired.IsChecked)
            mood.Add("Tired");
        return mood.ToArray();
    }
}
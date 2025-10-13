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
        //store mood selection
        var selectedRadioButton = MoodFlexLayout.Children
                                    .OfType<RadioButton>()
                                    .FirstOrDefault(rb => rb.IsChecked);

        var result = new FeelingResult
        {
            Rating = RatingSlider.Value,
            Mood = selectedRadioButton?.Value as string ?? "Unknown"
        };

        Close(result);
    }
}
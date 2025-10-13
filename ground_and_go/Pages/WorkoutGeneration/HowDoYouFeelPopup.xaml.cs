// Samuel Reynebeau
using CommunityToolkit.Maui.Views;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class HowDoYouFeelPopup : Popup
{
	public HowDoYouFeelPopup()
	{
		InitializeComponent();
	}

    private void OnCancel_Clicked(object sender, EventArgs e)
    {
        // This is a UI action: it simply closes the pop-up.
        Close();
    }

    private void OnSubmit_Clicked(object sender, EventArgs e)
    {
        // This button does nothing for now. I'll add the magic later.
    }
}
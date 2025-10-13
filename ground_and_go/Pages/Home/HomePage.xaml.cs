// Samuel Reynebeau
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.WorkoutGeneration;

namespace ground_and_go.Pages.Home;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}

    private async void OnStartWorkoutFlow_Clicked(object sender, EventArgs e)
    {
        // This will create and show the pop-up on the screen.
        await this.ShowPopupAsync(new HowDoYouFeelPopup());
    }

    private async void OnRestDay_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert("Navigation", "Rest Day Logged!", "OK");
    }
}

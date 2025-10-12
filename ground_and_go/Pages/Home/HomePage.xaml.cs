// Samuel Reynebeau
namespace ground_and_go.Pages.Home;

public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
	}

    private async void OnStartWorkoutFlow_Clicked(object sender, EventArgs e)
    {
        // TODO: This is where we will start the workout generation flow
        // await Navigation.PushAsync(new Pages.WorkoutGeneration.SomePage());
        await DisplayAlert("Navigation", "Workout Flow Started!", "OK");
    }

    private async void OnRestDay_Clicked(object sender, EventArgs e)
    {
        // TODO: This is where we will start the rest day flow
        await DisplayAlert("Navigation", "Rest Day Logged!", "OK");
    }
}

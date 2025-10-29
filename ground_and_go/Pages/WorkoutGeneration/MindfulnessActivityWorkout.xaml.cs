//Devlin Delegard
// FILE: ground_and_go/Pages/Workout/RestDayPage.xaml.cs
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.Home;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class MindfulnessActivityWorkoutPage : ContentPage
{
    public MindfulnessActivityWorkoutPage()
    {
        InitializeComponent();
    }

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // This button will eventually save the entry and navigate
        var popup = new WorkoutOptionsPopup();
        var result = await this.ShowPopupAsync(popup);

        // after the popup closes, navigate to the main "workout" tab
        // the "//" means go to this absolute route
        await Shell.Current.GoToAsync("//workout"); //TODO Add parameter to route to determine workout
    }
}
//Devlin Delegard
// FILE: ground_and_go/Pages/Workout/RestDayPage.xaml.cs
using ground_and_go.Pages.Home;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class MindfulnessActivityRestPage : ContentPage
{
    public MindfulnessActivityRestPage()
    {
        InitializeComponent();
    }

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // This button will eventually save the entry and navigate to the home page
        //TODO: This currently does not function properly, but it will do for now
        await Navigation.PushAsync(new HomePage());
    }

}
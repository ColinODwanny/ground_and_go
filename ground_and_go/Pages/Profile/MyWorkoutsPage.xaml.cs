// FILE: ground_and_go/Pages/Profile/MyWorkoutsPage.xaml.cs
namespace ground_and_go.Pages.Profile;

public partial class MyWorkoutsPage : ContentPage 
{
    public MyWorkoutsPage()
    {
        InitializeComponent();
    } 
    
    private async void OnWorkoutTapped(object sender, EventArgs e)
    {
        // use shell navigation with the registered route
        await Shell.Current.GoToAsync(nameof(ground_and_go.Pages.Profile.WorkoutPage));
    }
}
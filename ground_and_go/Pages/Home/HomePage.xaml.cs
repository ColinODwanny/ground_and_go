// Samuel Reynebeau
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.WorkoutGeneration;

namespace ground_and_go.Pages.Home;

public partial class HomePage : ContentPage
{
    //intialize the home page
    public HomePage()
    {
        InitializeComponent();
    }

    private async void OnStartWorkoutFlow_Clicked(object sender, EventArgs e)
    {
        //initiate popup and store result
        var popup = new HowDoYouFeelPopup();
        var result = await this.ShowPopupAsync(popup);

        //if rating/feeling is given then push to the journal entry page
        if (result is FeelingResult feelingResult)
        {
            // use shell navigation with the registered route
            // pass a parameter to tell the journal page this is a "workout" flow
            await Shell.Current.GoToAsync("WorkoutJournalEntry?flow=workout");
        }
    }

    private async void OnRestDay_Clicked(object sender, EventArgs e)
    {
        // same flow as the "begin" button
        var popup = new HowDoYouFeelPopup();
        var result = await this.ShowPopupAsync(popup);

        //if rating/feeling is given then push to the journal entry page
        if (result is FeelingResult feelingResult)
        {
            // pass a parameter to tell the journal page this is a "rest" flow
            await Shell.Current.GoToAsync("WorkoutJournalEntry?flow=rest");
        }
    }
}

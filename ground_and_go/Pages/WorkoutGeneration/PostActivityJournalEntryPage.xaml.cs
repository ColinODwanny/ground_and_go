// Samuel Reynebeau
namespace ground_and_go.Pages.WorkoutGeneration;

public partial class PostActivityJournalEntryPage : ContentPage
{
    public PostActivityJournalEntryPage()
    {
        InitializeComponent();
    }

    private async void OnFinish_Clicked(object sender, EventArgs e)
    {
        // This button will eventually save the entry and navigate home
        
        // navigate to the main home tab
        await Shell.Current.GoToAsync("//home");
    }
}
// Samuel Reynebeau
namespace ground_and_go.Pages.WorkoutGeneration;

public partial class JournalEntryPage : ContentPage
{
	public JournalEntryPage()
	{
		InitializeComponent();
	}

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // This button will eventually save the entry and navigate to the mindfulness activity
        await DisplayAlert("WIP", "Next step is not yet implemented.", "OK");
    }
}
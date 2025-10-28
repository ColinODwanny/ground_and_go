// Samuel Reynebeau
namespace ground_and_go.Pages.WorkoutGeneration;

// add this attribute to tell the page it can receive a "flow" parameter
[QueryProperty(nameof(FlowType), "flow")]
public partial class JournalEntryPage : ContentPage
{
    // this property will be set by shell
    public string? FlowType { get; set; }

    public JournalEntryPage()
    {
        InitializeComponent();
        BindingContext = MauiProgram.db;
    }

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // this button will eventually save the entry and navigate
        await MauiProgram.db.UploadJournalEntry(JournalEntry.Text);
        // check which flow we're in
        if (FlowType == "workout")
        {
            // go to the workout mindfulness page
            await Shell.Current.GoToAsync(nameof(MindfulnessActivityWorkoutPage));
        }
        else if (FlowType == "rest")
        {
            // go to the rest day mindfulness page
            await Shell.Current.GoToAsync(nameof(MindfulnessActivityRestPage));
        }
        else
        {
            // just in case
            await DisplayAlert("Error", "Could not determine navigation flow.", "OK");
        }
    }
}
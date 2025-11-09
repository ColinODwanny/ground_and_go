//Devlin Delegard
// FILE: ground_and_go/Pages/Workout/RestDayPage.xaml.cs
using ground_and_go.Pages.Home;

namespace ground_and_go.Pages.WorkoutGeneration;

// NEW: Add this QueryProperty
[QueryProperty(nameof(FlowType), "flow")]
public partial class MindfulnessActivityRestPage : ContentPage
{
    // NEW: Add this property
    public string? FlowType { get; set; }

    public MindfulnessActivityRestPage()
    {
        InitializeComponent();
    }

    // NEW: Add this method
    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // REST FLOW (4 steps total)
        // Step 2 (Journal) is done. This is Step 3.
        this.Title = "Step 3 of 4: Mindfulness";
        ProgressStepLabel.Text = "Step 3 of 4: Complete this activity";
        FlowProgressBar.Progress = 0.50; // 2/4 complete
    }

    // UPDATED: This method now passes the flow parameter
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // This button will eventually save the entry and navigate
        // change this to go to the new post-activity journal page
        // NEW: We pass the 'flow' parameter along to the next page
        await Shell.Current.GoToAsync($"{nameof(PostActivityJournalEntryPage)}?flow=rest");
    }
}
//Devlin Delegard
// FILE: ground_and_go/Pages/Workout/RestDayPage.xaml.cs
using ground_and_go.Pages.Home;

namespace ground_and_go.Pages.WorkoutGeneration;

[QueryProperty(nameof(FlowType), "flow")]
public partial class MindfulnessActivityRestPage : ContentPage
{
    public string? FlowType { get; set; }

    public MindfulnessActivityRestPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // REST FLOW (4 steps total)
        // Step 2 (Journal) is done. This is Step 3.
        this.Title = "Step 3 of 4: Mindfulness";
        ProgressStepLabel.Text = "Step 3 of 4: Complete this activity";
        FlowProgressBar.Progress = 0.50; // 2/4 complete
    }

    // This method passes the flow parameter
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // Navigate hierarchically using the alias "PostJournal"
        await Shell.Current.GoToAsync("PostJournal");
    }
}
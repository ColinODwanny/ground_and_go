//Devlin Delegard
// FILE: ground_and_go/Pages/Workout/RestDayPage.xaml.cs
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.Home;

namespace ground_and_go.Pages.WorkoutGeneration;

// NEW: Add this QueryProperty
[QueryProperty(nameof(FlowType), "flow")]
public partial class MindfulnessActivityWorkoutPage : ContentPage
{
    // NEW: Add this property
    public string? FlowType { get; set; }

    public MindfulnessActivityWorkoutPage()
    {
        InitializeComponent();
    }

    // NEW: Add this method
    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // WORKOUT FLOW (5 steps total)
        // Step 2 (Journal) is done. This is Step 3.
        this.Title = "Step 3 of 5: Mindfulness";
        ProgressStepLabel.Text = "Step 3 of 5: Complete this activity";
        FlowProgressBar.Progress = 0.40; // 2/5 complete
    }

    // UPDATED: This method now passes the flow parameter
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // This button will eventually save the entry and navigate
        var popup = new WorkoutOptionsPopup();
        var result = await this.ShowPopupAsync(popup);

        // after the popup closes, navigate to the main "workout" tab
        // the "//" means go to this absolute route
        // NEW: We pass the 'flow' parameter along to the next page
        await Shell.Current.GoToAsync($"//workout?flow=workout"); 
    }
}
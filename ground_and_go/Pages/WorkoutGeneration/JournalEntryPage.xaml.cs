// Samuel Reynebeau
using System.Text.Json;
using ground_and_go.Services;
using ground_and_go.Models;  

namespace ground_and_go.Pages.WorkoutGeneration;


// add this attribute to tell the page the results of the popup
[QueryProperty(nameof(ResultJSON), "results")]
public partial class JournalEntryPage : ContentPage
{
    // Add private fields for our services
    private readonly Database _database;
    private readonly MockAuthService _authService;
    // Add a field for the progress service
    private readonly DailyProgressService _progressService;
        
    public FeelingResult? ResultType { get; set; }
    private string? _resultJSON;
    public string? ResultJSON
    {
        get => _resultJSON;
        set
        {
            _resultJSON = value;
            ResultType = JsonSerializer.Deserialize<FeelingResult>(_resultJSON!); //Converts the JSON into the feelings inputted by the user
        }
    }

    public JournalEntryPage(Database database, MockAuthService authService, DailyProgressService progressService)
    {
        InitializeComponent();

        _database = database;
        _authService = authService;
        // Assign the new service ***
        _progressService = progressService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // This makes the progress bar show the correct step
        // based on the flow started on the HomePage
        // Read the flow type directly from the service
        if (_progressService.CurrentFlowType == "workout")
        {
            // WORKOUT FLOW (5 steps total)
            // Step 1 (Popup) is done. This is Step 2.
            this.Title = "Step 2 of 5: Journal";
            ProgressStepLabel.Text = "Step 2 of 5: Write a reflection";
            FlowProgressBar.Progress = 0.20; // 1/5 complete
        }
        else // "rest" flow
        {
            // REST FLOW (4 steps total)
            // Step 1 (Popup) is done. This is Step 2.
            this.Title = "Step 2 of 4: Journal";
            ProgressStepLabel.Text = "Step 2 of 4: Write a reflection";
            FlowProgressBar.Progress = 0.25; // 1/4 complete
        }
    }

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // 1. Get the real user ID from the database service
        string? memberId = _database.GetAuthenticatedMemberId();

        // 1a. check if user is actually logged in
        if (string.IsNullOrEmpty(memberId))
        {
            await DisplayAlert("Error", "You are not logged in. Please restart the app.", "OK");
            return;
        }
        
        // 2. Save the initial journal entry to the database
        // This creates the log for today and sets our database progress
        await _database.CreateInitialWorkoutLog(memberId, JournalEntry.Text);
        
        // 3. Navigate to the next page
        // Read the flow type directly from the service
        if (_progressService.CurrentFlowType == "workout")
        {
            // Navigate without query parameters
            await Shell.Current.GoToAsync($"{nameof(MindfulnessActivityWorkoutPage)}");
        }
        else if (_progressService.CurrentFlowType == "rest")
        {
            // Navigate without query parameters
            await Shell.Current.GoToAsync($"{nameof(MindfulnessActivityRestPage)}");
        }
        else
        {
            // just in case
            await DisplayAlert("Error", "Could not determine navigation flow.", "OK");
        }
    }
}
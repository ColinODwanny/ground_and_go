// Samuel Reynebeau
using System.Text.Json;
using ground_and_go.Services; // NEW: Import our services
using ground_and_go.Models;   // NEW: Import models

namespace ground_and_go.Pages.WorkoutGeneration;

// *** FIX: Removed the QueryProperty for FlowType ***
// add this attribute to tell the page it can receive a "flow" parameter
// [QueryProperty(nameof(FlowType), "flow")]
// add this attribute to tell the page the results of the popup
[QueryProperty(nameof(ResultJSON), "results")]
public partial class JournalEntryPage : ContentPage
{
    // NEW: Add private fields for our services
    private readonly Database _database;
    private readonly MockAuthService _authService;
    // *** FIX: Add a field for the progress service ***
    private readonly DailyProgressService _progressService;
        
    // *** FIX: Removed this property, we will read from the service instead ***
    // public string? FlowType { get; set; }
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

    // *** FIX: Update constructor to receive our services ***
    public JournalEntryPage(Database database, MockAuthService authService, DailyProgressService progressService)
    {
        InitializeComponent();

        _database = database;
        _authService = authService;
        // *** FIX: Assign the new service ***
        _progressService = progressService;
    }

    // NEW: Add this OnAppearing method
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // This makes the progress bar show the correct step
        // based on the flow started on the HomePage
        // *** FIX: Read the flow type directly from the service ***
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


    // UPDATED: This 'OnNext_Clicked' method is modified
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // 1. Get the (mock) user ID
        int memberId = _authService.GetCurrentMemberId();
        
        // 2. Save the initial journal entry to the database
        // This creates the log for today and sets our database progress
        await _database.CreateInitialWorkoutLog(memberId, JournalEntry.Text);
        
        // 3. Navigate to the next page
        // *** FIX: Read the flow type directly from the service ***
        if (_progressService.CurrentFlowType == "workout")
        {
            // *** FIX: Navigate without query parameters ***
            await Shell.Current.GoToAsync($"{nameof(MindfulnessActivityWorkoutPage)}");
        }
        else if (_progressService.CurrentFlowType == "rest")
        {
            // *** FIX: Navigate without query parameters ***
            await Shell.Current.GoToAsync($"{nameof(MindfulnessActivityRestPage)}");
        }
        else
        {
            // just in case
            await DisplayAlert("Error", "Could not determine navigation flow.", "OK");
        }
    }
}
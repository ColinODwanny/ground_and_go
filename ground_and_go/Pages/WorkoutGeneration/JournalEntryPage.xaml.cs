// Samuel Reynebeau
using System.Text.Json;
using ground_and_go.Services; // NEW: Import our services
using ground_and_go.Models;   // NEW: Import models

namespace ground_and_go.Pages.WorkoutGeneration;

// add this attribute to tell the page it can receive a "flow" parameter
[QueryProperty(nameof(FlowType), "flow")]
// add this attribute to tell the page the results of the popup
[QueryProperty(nameof(ResultJSON), "results")]
public partial class JournalEntryPage : ContentPage
{
    // NEW: Add private fields for our services
    private readonly Database _database;
    private readonly MockAuthService _authService;
    
    // this property will be set by shell
    public string? FlowType { get; set; }
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

    // NEW: Update constructor to receive our services
    public JournalEntryPage(Database database, MockAuthService authService)
    {
        InitializeComponent();

        _database = database;
        _authService = authService;
    }

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // 1. Get the (mock) user ID
        int memberId = _authService.GetCurrentMemberId();
        
        // 2. Save the initial journal entry to the database
        // This creates the log for today and sets our progress to Step 1
        await _database.CreateInitialWorkoutLog(memberId, JournalEntry.Text);
        
        // 3. Navigate to the next page
        // check which flow we're in
        if (FlowType == "workout")
        {
            // go to the workout mindfulness page
            await Shell.Current.GoToAsync(nameof(MindfulnessActivityWorkoutPage)); // Removed the bad parameter
        }
        else if (FlowType == "rest")
        {
            // go to the rest day mindfulness page
            await Shell.Current.GoToAsync(nameof(MindfulnessActivityRestPage)); // Removed the bad parameter
        }
        else
        {
            // just in case
            await DisplayAlert("Error", "Could not determine navigation flow.", "OK");
        }
    }

    // REMOVED: The 'calculateWorkoutId' function is no longer needed here.
    // The workout ID will be calculated and saved later in the flow.
}
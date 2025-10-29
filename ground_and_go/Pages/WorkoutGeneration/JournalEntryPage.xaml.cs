// Samuel Reynebeau
using System.Text.Json;

namespace ground_and_go.Pages.WorkoutGeneration;

// add this attribute to tell the page it can receive a "flow" parameter
[QueryProperty(nameof(FlowType), "flow")]
// add this attribute to tell the page the results of the popup
[QueryProperty(nameof(ResultJSON), "results")]
public partial class JournalEntryPage : ContentPage
{
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

    public JournalEntryPage()
    {
        //TODO Calculate the mindfulness activity to show, and implement them so that they display depending on the parameter passed
        InitializeComponent();
    }

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        // this button will eventually save the entry and navigate
        int workoutId = calculateWorkoutId(ResultType);
        await MauiProgram.db.UploadJournalEntry(JournalEntry.Text, workoutId);
        // check which flow we're in
        if (FlowType == "workout")
        {
            // go to the workout mindfulness page
            await Shell.Current.GoToAsync(nameof(MindfulnessActivityWorkoutPage) + "?feelings=ResultType");
        }
        else if (FlowType == "rest")
        {
            // go to the rest day mindfulness page
            await Shell.Current.GoToAsync(nameof(MindfulnessActivityRestPage) + "?feelings=ResultType");
        }
        else
        {
            // just in case
            await DisplayAlert("Error", "Could not determine navigation flow.", "OK");
        }
    }

    private int calculateWorkoutId(FeelingResult? ResultType)
    {
        //rating < 5 or (anxious + sad + tired + depressed) > (energized + angry + happy) = Meditation or Low Impact or Yoga
        //rating >= 5 and (energized + angry + happy) > (anxious + sad + tired + depressed) = High Impact or Cardio or Low Impact

        return 201; //Placeholder
        //TODO do some calculation to determine workout to be given using inputted feelings - more workouts + exercises will be needed in the database for this
    }
}
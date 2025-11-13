// Samuel Reynebeau
using System.Text.Json;
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.WorkoutGeneration;
using ground_and_go.Services; 

namespace ground_and_go.Pages.Home;

public partial class HomePage : ContentPage
{
    private readonly DailyProgressService _progressService;

    public HomePage(DailyProgressService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 1. Show spinner and hide all content panels
        LoadingSpinner.IsVisible = true;
        ProgressContainer.IsVisible = false;
        StartFlowContainer.IsVisible = false;
        RestDayContainer.IsVisible = false;

        // 2. Run the check in the background (fire and forget)
        // This avoids blocking the UI thread and allows the
        // MainThread.BeginInvokeOnMainThread to work correctly.
        _ = CheckDailyProgress();
    }

    private async Task CheckDailyProgress()
    {
        // This code now runs on a background thread
        var progressState = await _progressService.GetTodaysProgressAsync();

        // 3. Force all UI updates to run on the Main UI Thread
        // We use BeginInvoke which is the correct "fire and forget"
        // for UI updates from a background thread.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (progressState.Step == 0)
            {
                // STEP 0: Not Started
                StartFlowContainer.IsVisible = true;
                RestDayContainer.IsVisible = true;
            }
            else if (progressState.Step > 0 && progressState.Step < 3)
            {
                // STEP 1 or 2: In Progress
                ProgressContainer.IsVisible = true;
                
                WorkoutProgressBar.Progress = progressState.Progress;
                ResumeButton.CommandParameter = progressState;
                
                ProgressLabel.Text = "You've already started today's activity!";
                ResumeButton.IsVisible = true;
            }
            else // Step 3
            {
                // STEP 3: All Complete
                ProgressContainer.IsVisible = true;
                
                WorkoutProgressBar.Progress = 1.0;
                ProgressLabel.Text = "You've completed your activity for today!";
                ResumeButton.IsVisible = false; 
            }

            // 4. Hide the spinner once the correct content is ready
            LoadingSpinner.IsVisible = false;
        });
    }

    // This is your existing function.
    private async void OnStartWorkoutFlow_Clicked(object sender, EventArgs e)
    {
        //initiate popup and store result
        var popup = new HowDoYouFeelPopup();
        var result = await this.ShowPopupAsync(popup);


        //if rating/feeling is given then push to the journal entry page
        if (result is FeelingResult)
        {
            //Convert result to JSON to use in future pages
            var resultJSON = JsonSerializer.Serialize(result);

            _progressService.CurrentFlowType = "workout";

            // use shell navigation with the registered route
            // pass a parameter to tell the journal page this is a "workout" flow
            try
            {
                await Shell.Current.GoToAsync($"WorkoutJournalEntry?flow=workout&results={resultJSON}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ATTN: Error while accessing workout journal entry (Workout) -- {ex.ToString()}");
            }

        }
    }

    // This is your existing function.
    private async void OnRestDay_Clicked(object sender, EventArgs e)
    {
        // same flow as the "begin" button
          var popup = new HowDoYouFeelPopup();
          var result = await this.ShowPopupAsync(popup);

          //if rating/feeling is given then push to the journal entry page
          if (result is FeelingResult)
          {
                //Convert result to JSON to use in future pages
                var resultJSON = JsonSerializer.Serialize(result);

                // pass a parameter to tell the journal page this is a "rest" flow
                try
                { 
                    _progressService.CurrentFlowType = "rest";

                    await Shell.Current.GoToAsync($"WorkoutJournalEntry?results={resultJSON}");
                }
                  catch (Exception ex)
                {
                    Console.WriteLine($"ATTN: Error while accessing workout journal entry (Rest) -- {ex.ToString()}");
                } 
                
          }
    }

    private async void OnResume_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not DailyProgressState state)
            return;

        // Navigate to the correct page based on the step
        if (state.Step == 1)
        {
            // User finished journal, needs to do mindfulness.
            await Shell.Current.GoToAsync(nameof(MindfulnessActivityWorkoutPage));
        }
        else if (state.Step == 2)
        {
            // User has a workout, needs to perform it
            await Shell.Current.GoToAsync("TheWorkout");
        }
    }
}
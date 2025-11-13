// Samuel Reynebeau
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.WorkoutGeneration;
using ground_and_go.Services; 
using System.Text.Json;

namespace ground_and_go.Pages.Home;

public partial class HomePage : ContentPage, INotifyPropertyChanged
{
    private readonly DailyProgressService _progressService;

    // Define the variables for our UI state
    private bool _isLoading = true;
    private bool _showStart = false;
    private bool _showResume = false;
    private bool _showComplete = false;
    private double _progressValue = 0.0;

    // Create Public Properties that the UI can "Bind" to
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool ShowStart
    {
        get => _showStart;
        set { _showStart = value; OnPropertyChanged(); }
    }

    public bool ShowResume
    {
        get => _showResume;
        set { _showResume = value; OnPropertyChanged(); }
    }

    public bool ShowComplete
    {
        get => _showComplete;
        set { _showComplete = value; OnPropertyChanged(); }
    }

    public double ProgressValue
    {
        get => _progressValue;
        set { _progressValue = value; OnPropertyChanged(); }
    }

    public HomePage(DailyProgressService progressService)
    {
        InitializeComponent();
        _progressService = progressService;
        
        // Tell the XAML that "this" file holds the data
        BindingContext = this;
    }

    // Use OnAppearing to refresh the data every time the page shows up
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshData();
    }

    private async Task RefreshData()
    {
        // Switch to loading state
        IsLoading = true;
        ShowStart = false;
        ShowResume = false;
        ShowComplete = false;

        // Get the data
        var progressState = await _progressService.GetTodaysProgressAsync();

        // Update the specific state based on the data
        if (progressState.Step == 0)
        {
            // Not Started
            ShowStart = true;
        }
        else if (progressState.Step > 0 && progressState.Step < 3)
        {
            // In Progress
            ShowResume = true;
            ProgressValue = progressState.Progress;
            ResumeButton.CommandParameter = progressState;
        }
        else
        {
            // Complete
            ShowComplete = true;
            ProgressValue = 1.0;
        }

        // Turn off loading
        IsLoading = false;
    }

    // 2. ADDED '?' TO THE EVENT HANDLER
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Event Handlers

    private async void OnStartWorkoutFlow_Clicked(object sender, EventArgs e)
    {
        var popup = new HowDoYouFeelPopup();
        var result = await this.ShowPopupAsync(popup);

        if (result is FeelingResult)
        {
            var resultJSON = JsonSerializer.Serialize(result);
            _progressService.CurrentFlowType = "workout";

            try
            {
                await Shell.Current.GoToAsync($"WorkoutJournalEntry?flow=workout&results={resultJSON}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }
    }

    private async void OnRestDay_Clicked(object sender, EventArgs e)
    {
          var popup = new HowDoYouFeelPopup();
          var result = await this.ShowPopupAsync(popup);

          if (result is FeelingResult)
          {
                var resultJSON = JsonSerializer.Serialize(result);
                try
                { 
                    _progressService.CurrentFlowType = "rest";
                    await Shell.Current.GoToAsync($"WorkoutJournalEntry?results={resultJSON}");
                }
                  catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex}");
                } 
          }
    }

    private async void OnResume_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not DailyProgressState state)
            return;

        if (state.Step == 1)
        {
            await Shell.Current.GoToAsync(nameof(MindfulnessActivityWorkoutPage));
        }
        else if (state.Step == 2)
        {
            await Shell.Current.GoToAsync("TheWorkout");
        }
    }
}
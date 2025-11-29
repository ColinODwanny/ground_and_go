// FILE: ground_and_go/Pages/Home/HomePage.xaml.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;
using ground_and_go.Pages.WorkoutGeneration;
using ground_and_go.Services; 
using System.Text.Json;
using ground_and_go.Models;

namespace ground_and_go.Pages.Home;

public partial class HomePage : ContentPage, INotifyPropertyChanged
{
    private readonly DailyProgressService _progressService;
    private readonly Database _database; 
    
    private bool _isLoading = true;
    private bool _showStart = false;
    private bool _showResume = false;
    private bool _showComplete = false;
    private double _progressValue = 0.0;

    public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }
    public bool ShowStart { get => _showStart; set { _showStart = value; OnPropertyChanged(); } }
    public bool ShowResume { get => _showResume; set { _showResume = value; OnPropertyChanged(); } }
    public bool ShowComplete { get => _showComplete; set { _showComplete = value; OnPropertyChanged(); } }
    public double ProgressValue { get => _progressValue; set { _progressValue = value; OnPropertyChanged(); } }

    public HomePage(DailyProgressService progressService, Database database)
    {
        InitializeComponent();
        _progressService = progressService;
        _database = database; 
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshData();
    }

    private async Task RefreshData()
    {
        IsLoading = true;
        ShowStart = false; ShowResume = false; ShowComplete = false;

        // The Service now handles all recovery logic internally!
        // We just ask "What Step are we on?"
        var progressState = await _progressService.GetTodaysProgressAsync();

        if (progressState.Step == 0)
        {
            ShowStart = true;
        }
        else if (progressState.Step < 5)
        {
            ShowResume = true;
            ProgressValue = progressState.Progress;
            ResumeButton.CommandParameter = progressState;
        }
        else
        {
            ShowComplete = true;
            ProgressValue = 1.0;
        }

        IsLoading = false;
    }

    private async void OnResume_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not DailyProgressState state) return;

        // 1. Get Configuration from Service
        bool includesMindfulness = await _progressService.RequiresMindfulnessAsync();
        string currentFlow = _progressService.CurrentFlowType;
        int currentStep = state.Step;

        // 2. Route based on Actual Steps
        
        // --- STEP 1 (Should not be reachable) ---
        if (currentStep == 1) 
        {
             await DisplayAlert("Resume", "Please start a new activity.", "OK");
        }
        
        // --- STEP 2: ON JOURNAL PAGE ---
        else if (currentStep == 2)
        {
            // Resume to Journal Page. We need to pass the result json so it renders the header.
            var result = _progressService.CurrentFeelingResult;
            var json = JsonSerializer.Serialize(result);
            await Shell.Current.GoToAsync($"WorkoutJournalEntry?flow={currentFlow}&results={json}");
        }

        // --- STEP 3: GOING TO MINDFULNESS OR NEXT ACTIVITY ---
        else if (currentStep == 3)
        {
            if (includesMindfulness)
            {
                if (currentFlow == "rest") await Shell.Current.GoToAsync(nameof(MindfulnessActivityRestPage));
                else await Shell.Current.GoToAsync(nameof(MindfulnessActivityWorkoutPage));
            }
            else
            {
                if (currentFlow == "rest") await Shell.Current.GoToAsync("PostJournal");
                else await Shell.Current.GoToAsync("TheWorkout");
            }
        }

        // --- STEP 4: GOING TO WORKOUT OR POST-JOURNAL ---
        else if (currentStep == 4)
        {
            if (currentFlow == "rest") 
            {
                await Shell.Current.GoToAsync("PostJournal");
            }
            else 
            {
                await Shell.Current.GoToAsync("TheWorkout");
            }
        }
    }

    private async void OnStartWorkoutFlow_Clicked(object sender, EventArgs e)
    {
        if (await CheckIfComplete()) return;

        var popup = new HowDoYouFeelPopup("workout");
        var result = await this.ShowPopupAsync(popup);
        ProcessStartResult(result, "workout");
    }

    private async void OnRestDay_Clicked(object sender, EventArgs e)
    {
        if (await CheckIfComplete()) return;

        var popup = new HowDoYouFeelPopup("rest");
        var result = await this.ShowPopupAsync(popup);
        ProcessStartResult(result, "rest");
    }

    private async Task<bool> CheckIfComplete()
    {
        var progressState = await _progressService.GetTodaysProgressAsync();
        if (progressState.Step >= 5)
        {
            await DisplayAlert("Already Complete", "You've already completed your daily activity!", "OK");
            return true;
        }
        return false;
    }

    private async void ProcessStartResult(object? result, string flowType)
    {
        if (result is FeelingResult feelingResult)
        {
            var resultJSON = JsonSerializer.Serialize(result);
            
            // 1. Save to Service
            _progressService.CurrentFlowType = flowType;
            _progressService.CurrentFeelingResult = feelingResult;
            _progressService.CurrentWorkout = null; 

            // 2. SAVE TO DB IMMEDIATELY (Temp State)
            try 
            {
                var stateDict = new Dictionary<string, string> 
                { 
                    { "Flow", flowType }, 
                    { "Mood", feelingResult.Mood ?? "" } 
                };
                string stateJson = JsonSerializer.Serialize(stateDict);
                
                string? memberId = _database.GetAuthenticatedMemberId();
                if (memberId != null)
                {
                    // Call the new method in Database.cs
                    var log = await _database.StartDailyLog(memberId, stateJson);
                    if (log != null) _progressService.CurrentLogId = log.LogId;
                }
                
                await Shell.Current.GoToAsync($"WorkoutJournalEntry?flow={flowType}&results={resultJSON}");
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex}"); }
        }
    }
    
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
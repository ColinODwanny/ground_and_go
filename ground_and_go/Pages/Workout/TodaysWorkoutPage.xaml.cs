// FILE: ground_and_go/Pages/WorkoutGeneration/TodaysWorkoutPage.xaml.cs
// Aidan Trusky
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using ground_and_go.Services;
using ground_and_go.Models;
using ground_and_go.Components;
using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class TodaysWorkoutPage : ContentPage, INotifyPropertyChanged
{
    private Dictionary<string, Border> exerciseBorders;
    private Dictionary<string, WorkoutExerciseItem> exerciseData;
    private Models.Workout? _currentWorkout;
    private ground_and_go.Models.Workout? workout;

    // Fields for services
    private readonly DailyProgressService _progressService;
    private readonly Database _database;

    public Models.Workout? CurrentWorkout
    {
        get => _currentWorkout;
        set
        {
            _currentWorkout = value;
            OnPropertyChanged();
        }
    }

    // UPDATED CONSTRUCTOR: Inject both DailyProgressService AND Database
    public TodaysWorkoutPage(DailyProgressService progressService, Database database)
    {
        InitializeComponent();
        
        _progressService = progressService;
        _database = database; 
        
        // Initialize dictionaries
        exerciseBorders = new Dictionary<string, Border>();
        exerciseData = new Dictionary<string, WorkoutExerciseItem>();
        
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        Console.WriteLine("TodaysWorkoutPage OnAppearing started");
        
        // Hide hardcoded exercises immediately to prevent them from showing
        HideHardcodedExercises();
        
        // Use dynamic step counting based on emotion type from database
        // For workout, this is actual Step 4
        var (displayStep, totalSteps) = await _progressService.GetDisplayStepAsync(4); 
        double progress = await _progressService.GetProgressPercentageAsync(4);
        
        this.Title = $"Step {displayStep} of {totalSteps}: Your Workout";
        ProgressStepLabel.Text = $"Step {displayStep} of {totalSteps}: Complete your exercises";
        FlowProgressBar.Progress = progress;

        // Load the current workout from the database
        await LoadCurrentWorkout();
    }

    private void HideHardcodedExercises()
    {
        // Hide all the hardcoded static exercises immediately
        if (SquatBorder != null) SquatBorder.IsVisible = false;
        if (BenchPressBorder != null) BenchPressBorder.IsVisible = false;
        if (DeadliftBorder != null) DeadliftBorder.IsVisible = false;
        if (PullUpBorder != null) PullUpBorder.IsVisible = false;
        if (ShoulderPressBorder != null) ShoulderPressBorder.IsVisible = false;
    }

    private void ShowHardcodedExercises()
    {
        // Show the hardcoded static exercises as fallback
        if (SquatBorder != null) SquatBorder.IsVisible = true;
        if (BenchPressBorder != null) BenchPressBorder.IsVisible = true;
        if (DeadliftBorder != null) DeadliftBorder.IsVisible = true;
        if (PullUpBorder != null) PullUpBorder.IsVisible = true;
        if (ShoulderPressBorder != null) ShoulderPressBorder.IsVisible = true;
    }

    private async void OnBeginExerciseClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not Button button) return;

            var exerciseName = button.CommandParameter?.ToString();
            if (string.IsNullOrEmpty(exerciseName)) return;

            // Show the exercise detail popup with exercise data if available
            var workoutExerciseData = exerciseData.ContainsKey(exerciseName) ? exerciseData[exerciseName] : null;
            var popup = new ExerciseDetailPopup(exerciseName, workoutExerciseData);
            var result = await this.ShowPopupAsync(popup);

            // If exercise was marked as complete, change the border color
            if (result is string completedExercise && exerciseBorders.ContainsKey(completedExercise))
            {
                exerciseBorders[completedExercise].BackgroundColor = Color.FromArgb("#C8E6C9");
                exerciseBorders[completedExercise].Stroke = Color.FromArgb("#4CAF50"); // Green border
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    // UPDATED: Saves "STATE:Pending" before navigation to fix Resume logic
    private async void OnCompleteWorkout_Clicked(object? sender, EventArgs e)
    {
        string? logId = _progressService.CurrentLogId;
        
        if (!string.IsNullOrEmpty(logId))
        {
            // SAVE TEMP STATE "STATE:Pending" to `after_journal`
            // This signals that the workout is done (Step 4), but the journal is pending (Step 5).
            await _database.UpdateAfterJournalAsync(logId, "STATE:Pending");
        }

        // navigate to the new post-activity journal page
        await Shell.Current.GoToAsync("PostJournal");
    }

    private async Task LoadCurrentWorkout()
    {
        try
        {
            Console.WriteLine($"LoadCurrentWorkout started");
            
            // Get the current user's member ID
            string? memberId = _database.GetAuthenticatedMemberId();
            
            if (string.IsNullOrEmpty(memberId))
            {
                await DisplayAlert("Error", "User not logged in.", "OK");
                return;
            }

            // Try to get today's workout log directly
            var todaysLog = await _database.GetTodaysWorkoutLog(memberId);
            
            if (todaysLog?.WorkoutId == null || todaysLog.WorkoutId <= 0)
            {
                // Try service as fallback if database has no workout
                if (_progressService.CurrentWorkout != null && _progressService.CurrentWorkout.WorkoutId != 999)
                {
                    CurrentWorkout = _progressService.CurrentWorkout;
                    return;
                }
                
                await DisplayAlert("Error", "No workout assigned for today.", "OK");
                return;
            }

            // Update the service with the current log ID if it was missing
            if (string.IsNullOrEmpty(_progressService.CurrentLogId))
            {
                _progressService.CurrentLogId = todaysLog.LogId;
            }
            
            // Get the workout details with loading indicator
            if(workout == null){
                var loadingPopup = new LoadingPopup("Loading your workout...");
                this.ShowPopup(loadingPopup);
                
                
                // Give the popup time to show before starting heavy work
                await Task.Delay(50);
                
                try
                {
                    workout = await _database.GetWorkoutById(todaysLog.WorkoutId.Value);
                    
                    // Set the workout and load UI sections
                    CurrentWorkout = workout;
                    await LoadWorkoutSections();
                    
                    // Give UI time to finish rendering before closing popup
                    await Task.Delay(50);
                } catch (Exception e)
                    {
                        await DisplayAlert("Technical Difficulties", "There was an issue loading your workout from the Database", "OK");
                    }
                finally
                {
                    loadingPopup.Close();
                }
            }
            
            
            
            // Check if workout has exercises in any format
            bool hasSections = CurrentWorkout?.Exercises?.Sections?.Count > 0;
            bool hasDirectExercises = CurrentWorkout?.Exercises?.Exercises?.Count > 0;
            if (!hasSections && !hasDirectExercises)
            {
                Console.WriteLine($"Warning: Workout {CurrentWorkout?.WorkoutId} has no exercises in any format");
            }
            
            if (CurrentWorkout == null)
            {
                await DisplayAlert("Error", "Could not load workout details.", "OK");
            }
            else
            {
                // Update the service with the real workout to avoid future issues
                _progressService.CurrentWorkout = CurrentWorkout;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in LoadCurrentWorkout: {ex.Message}");
            await DisplayAlert("Error", $"Error loading workout: {ex.Message}", "OK");
        }
    }

    private async Task LoadWorkoutSections()
    {
        if (CurrentWorkout == null || CurrentWorkout.Exercises == null)
        {
            Console.WriteLine("CurrentWorkout is null - showing hardcoded exercises");
            ShowHardcodedExercises();
            return;
        }
        
        // Check if we have sections or direct exercises
        bool hasSections = CurrentWorkout.Exercises.Sections != null && CurrentWorkout.Exercises.Sections.Count > 0;
        bool hasDirectExercises = CurrentWorkout.Exercises.Exercises != null && CurrentWorkout.Exercises.Exercises.Count > 0;
        
        if (!hasSections && !hasDirectExercises)
        {
            Console.WriteLine($"No sections or exercises found - showing hardcoded exercises");
            ShowHardcodedExercises();
            return;
        }
        
        if (hasDirectExercises && !hasSections)
        {
            // Added Null Coalescing operators (?. and ??) to fix the CS8602 warning
            int count = CurrentWorkout?.Exercises?.Exercises?.Count ?? 0;
            Console.WriteLine($"Loading {count} direct exercises (no sections format)");
            await LoadDirectExercises();
            return;
        }

        Console.WriteLine($"Loading {CurrentWorkout?.Exercises?.Sections?.Count} workout sections");

        // Clear existing content and rebuild dynamically
        WorkoutContentStack.Children.Clear();
        exerciseBorders.Clear();
        exerciseData.Clear();

        // Debug: Check what exercises we have in the database
        await DebugAvailableExercises();
        
        foreach (var section in CurrentWorkout.Exercises.Sections)
        {
            // Create a container for this section
            var sectionContainer = new Border
            {
                Stroke = Colors.LightGray,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                BackgroundColor = Colors.White,
                Margin = new Thickness(0, 10, 0, 10),
                Padding = new Thickness(15)
            };

            sectionContainer.Shadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(2, 2),
                Opacity = 0.1f
            };

            var sectionLayout = new VerticalStackLayout { Spacing = 10 };

            // Add section header
            var sectionHeader = new Label
            {
                Text = section.Title,
                FontSize = 22,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                Margin = new Thickness(0, 0, 0, 5)
            };
            sectionLayout.Children.Add(sectionHeader);

            // Add section note if available
            if (!string.IsNullOrEmpty(section.Note))
            {
                var noteLabel = new Label
                {
                    Text = section.Note,
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    FontAttributes = FontAttributes.Italic,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                sectionLayout.Children.Add(noteLabel);
            }

            // Add exercises for this section
            if (section.Exercises != null)
            {
                foreach (var exercise in section.Exercises)
                {
                    await AddExerciseToSection(exercise, section.Type, sectionLayout);
                }
            }

            sectionContainer.Content = sectionLayout;
            WorkoutContentStack.Children.Add(sectionContainer);
        }
    }

    private async Task LoadDirectExercises()
    {
        if (CurrentWorkout == null || CurrentWorkout.Exercises == null) return;

        // Clear existing content and rebuild dynamically
        WorkoutContentStack.Children.Clear();
        exerciseBorders.Clear();
        exerciseData.Clear();

        // Debug: Check what exercises we have in the database
        await DebugAvailableExercises();
        
        // Create a single section container for direct exercises
        var sectionContainer = new Border
        {
            Stroke = Colors.LightGray,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            BackgroundColor = Colors.White,
            Margin = new Thickness(0, 10, 0, 10),
            Padding = new Thickness(15)
        };

        sectionContainer.Shadow = new Shadow
        {
            Brush = Colors.Black,
            Offset = new Point(2, 2),
            Opacity = 0.1f
        };

        var sectionLayout = new VerticalStackLayout
        {
            Spacing = 10
        };

        // Add workout description if available
        if (!string.IsNullOrEmpty(CurrentWorkout.Exercises.Description))
        {
            var descriptionLabel = new Label
            {
                Text = CurrentWorkout.Exercises.Description,
                TextColor = Colors.Gray,
                FontSize = 14,
                FontAttributes = FontAttributes.Italic,
                Margin = new Thickness(0, 0, 0, 15)
            };
            sectionLayout.Children.Add(descriptionLabel);
        }

        // Add exercises
        if (CurrentWorkout.Exercises.Exercises != null)
        {
            foreach (var exercise in CurrentWorkout.Exercises.Exercises)
            {
                await AddExerciseToSection(exercise, "direct", sectionLayout);
            }
        }

        sectionContainer.Content = sectionLayout;
        WorkoutContentStack.Children.Add(sectionContainer);
    }

    private async Task DebugAvailableExercises()
    {
        try
        {
            var exercisesDict = await _database.GetAllExercises();
            var exerciseInfo = $"DB Exercises ({exercisesDict.Count}): ";
            if (exercisesDict.Count > 0)
            {
                exerciseInfo += string.Join(", ", exercisesDict.Keys.Take(15));
                if (exercisesDict.Count > 15) exerciseInfo += "...";
            }
            else
            {
                exerciseInfo += "NONE FOUND!";
            }
            
            Console.WriteLine($"Available exercises: {exerciseInfo}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting exercises: {ex.Message}");
        }
    }

    private async Task AddExerciseToSection(WorkoutExerciseItem exercise, string sectionType, VerticalStackLayout sectionLayout)
    {
        try
        {
            // Get exercise details from database
            var exercisesDict = await _database.GetAllExercises();
            if (!exercisesDict.TryGetValue(exercise.Id, out var exerciseInfo))
            {
                Console.WriteLine($"Exercise with ID {exercise.Id} not found");
                
                // Add a placeholder exercise so user can see there's missing data
                var placeholderBorder = new Border
                {
                    Stroke = Colors.Red,
                    StrokeThickness = 2,
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    BackgroundColor = Colors.LightPink,
                    HeightRequest = 40,
                    Margin = new Thickness(0, 5)
                };

                var placeholderLabel = new Label
                {
                    Text = $"Missing Exercise (ID: {exercise.Id}) - Sets: {exercise.SetsDisplay}, Reps: {exercise.Reps}",
                    FontSize = 12,
                    TextColor = Colors.DarkRed,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };

                placeholderBorder.Content = placeholderLabel;
                sectionLayout.Children.Add(placeholderBorder);
                return;
            }

            // Create exercise UI element
            var border = new Border
            {
                Stroke = Colors.LightGray,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                BackgroundColor = Color.FromArgb("#FAFAFA"),
                HeightRequest = 60,
                Margin = new Thickness(0, 3)
            };

            border.Shadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(1, 1),
                Opacity = 0.1f
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 8, 0)
            };

            var exerciseInfoStack = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Spacing = 2
            };

            var nameLabel = new Label
            {
                Text = exerciseInfo.Name,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black
            };

            var detailsText = BuildExerciseDetailsText(exercise, sectionType);
            var detailsLabel = new Label
            {
                Text = detailsText,
                FontSize = 14,
                TextColor = Colors.DarkGray
            };

            exerciseInfoStack.Children.Add(nameLabel);
            if (!string.IsNullOrEmpty(detailsText))
            {
                exerciseInfoStack.Children.Add(detailsLabel);
            }

            var beginButton = new Button
            {
                Text = "Begin",
                BackgroundColor = Color.FromArgb("#2196F3"),
                TextColor = Colors.White,
                CornerRadius = 12,
                WidthRequest = 70,
                HeightRequest = 32,
                FontSize = 14,
                CommandParameter = exerciseInfo.Name
            };

            beginButton.Clicked += OnBeginExerciseClicked;

            grid.Children.Add(exerciseInfoStack);
            grid.Children.Add(beginButton);
            Grid.SetColumn(beginButton, 1);

            border.Content = grid;
            sectionLayout.Children.Add(border);

            // Store in dictionary for completion tracking
            exerciseBorders[exerciseInfo.Name] = border;
            exerciseData[exerciseInfo.Name] = exercise;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding exercise to section: {ex.Message}");
        }
    }

    private string BuildExerciseDetailsText(WorkoutExerciseItem exercise, string sectionType)
    {
        var details = new List<string>();

        if (!string.IsNullOrEmpty(exercise.SetsDisplay))
            details.Add($"{exercise.SetsDisplay} sets");

        if (!string.IsNullOrEmpty(exercise.Reps))
            details.Add($"{exercise.Reps} reps");

        if (!string.IsNullOrEmpty(exercise.Duration))
            details.Add($"{exercise.Duration}");

        return details.Count > 0 ? string.Join(" • ", details) : "";
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
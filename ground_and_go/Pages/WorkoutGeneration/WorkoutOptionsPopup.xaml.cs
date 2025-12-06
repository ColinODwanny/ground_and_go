// Samuel Reynebeau
using CommunityToolkit.Maui.Views;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class WorkoutOptionsPopup : Popup
{
    private Button? _selectedWorkoutTypeButton;
    private Button? _selectedEquipmentButton;
    private readonly List<string> _availableWorkoutTypes;
    private readonly string _userEmotion;
    private readonly Database _database;

    public WorkoutOptionsPopup(List<string> availableWorkoutTypes, string userEmotion, Database database)
    {
        _availableWorkoutTypes = availableWorkoutTypes ?? new List<string> { "Strength Training", "Cardio" };
        _userEmotion = userEmotion;
        _database = database;
        InitializeComponent();
        CreateWorkoutTypeButtons();
    }

    // Keep backward compatibility
    public WorkoutOptionsPopup() : this(new List<string> { "Strength Training", "Cardio" }, "", null!)
    {
    }

    // Backward compatibility with just workout types
    public WorkoutOptionsPopup(List<string> availableWorkoutTypes) : this(availableWorkoutTypes, "", null!)
    {
    }

    private void CreateWorkoutTypeButtons()
    {
        WorkoutTypeButtonsLayout.Children.Clear();

        foreach (var workoutType in _availableWorkoutTypes)
        {
            var button = new Button
            {
                Text = workoutType,
                Margin = new Thickness(5),
                BackgroundColor = Color.FromArgb("#F3F4F6"),
                TextColor = Colors.Black,
                BorderColor = Color.FromArgb("#E0E0E0"),
                BorderWidth = 2,
                CornerRadius = 20,
                WidthRequest = 160,
                Padding = new Thickness(12, 6)
            };

            button.Clicked += OnWorkoutTypeClicked;
            WorkoutTypeButtonsLayout.Children.Add(button);
        }
    }

    private void OnCancel_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    private void OnWorkoutTypeClicked(object? sender, EventArgs e)
    {
        if (sender is Button clicked)
        {
            // Deselect previous
            if (_selectedWorkoutTypeButton != null)
            {
                _selectedWorkoutTypeButton.BackgroundColor = Color.FromArgb("#F3F4F6");
                _selectedWorkoutTypeButton.TextColor = Colors.Black;
                _selectedWorkoutTypeButton.BorderColor = Color.FromArgb("#E0E0E0");
            }

            // Select new
            _selectedWorkoutTypeButton = clicked;
            _selectedWorkoutTypeButton.BackgroundColor = Color.FromArgb("#2196F3");
            _selectedWorkoutTypeButton.TextColor = Colors.White;
            _selectedWorkoutTypeButton.BorderColor = Color.FromArgb("#2196F3");
        }
    }

    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        if (_selectedWorkoutTypeButton == null)
        {
            await Shell.Current.DisplayAlert(
                "Missing selection",
                "Please choose your workout type.",
                "OK");
            return;
        }

        var workoutType = _selectedWorkoutTypeButton.Text;

        // Check if equipment selection is needed for this workout type
        bool needsEquipment = true;
        if (!string.IsNullOrEmpty(_userEmotion) && _database != null)
        {
            needsEquipment = await _database.IsEquipmentSelectionNeeded(_userEmotion, workoutType);
        }
        else
        {
            // Fallback logic for backward compatibility
            needsEquipment = workoutType != "Cardio";
        }

        if (!needsEquipment)
        {
            // Skip equipment selection and submit directly
            var result = new EquipmentResult
            {
                WorkoutType = workoutType,
                HomeAccess = false,
                GymAccess = false // Equipment doesn't matter for this category
            };
            Close(result);
        }
        else
        {
            // Show equipment selection
            WorkoutTypeStep.IsVisible = false;
            EquipmentStep.IsVisible = true;
        }
    }

    private void OnBack_Clicked(object sender, EventArgs e)
    {
        // Go back to workout type selection
        EquipmentStep.IsVisible = false;
        WorkoutTypeStep.IsVisible = true;
        
        // Reset equipment selection
        if (_selectedEquipmentButton != null)
        {
            _selectedEquipmentButton.BackgroundColor = Color.FromArgb("#F3F4F6");
            _selectedEquipmentButton.TextColor = Colors.Black;
            _selectedEquipmentButton.BorderColor = Color.FromArgb("#E0E0E0");
            _selectedEquipmentButton = null;
        }
    }

    private void OnEquipmentClicked(object sender, EventArgs e)
    {
        if (sender is Button clicked)
        {
            // Deselect previous
            if (_selectedEquipmentButton != null)
            {
                _selectedEquipmentButton.BackgroundColor = Color.FromArgb("#F3F4F6");
                _selectedEquipmentButton.TextColor = Colors.Black;
                _selectedEquipmentButton.BorderColor = Color.FromArgb("#E0E0E0");
            }

            // Select new
            _selectedEquipmentButton = clicked;
            _selectedEquipmentButton.BackgroundColor = Color.FromArgb("#2196F3");
            _selectedEquipmentButton.TextColor = Colors.White;
            _selectedEquipmentButton.BorderColor = Color.FromArgb("#2196F3");
        }
    }

    private async void OnSubmit_Clicked(object sender, EventArgs e)
    {
        if (_selectedEquipmentButton == null)
        {
            await Shell.Current.DisplayAlert(
                "Missing selection",
                "Please choose your equipment.",
                "OK");
            return;
        }

        var equipmentText = _selectedEquipmentButton.Text;
        var workoutType = _selectedWorkoutTypeButton?.Text ?? "Strength Training";

        var result = new EquipmentResult
        {
            WorkoutType = workoutType,
            HomeAccess = equipmentText == "Home Equipment",
            GymAccess = equipmentText == "Gym Equipment"
        };
        
        Close(result);
    }
}

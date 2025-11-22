// Samuel Reynebeau
using CommunityToolkit.Maui.Views;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class WorkoutOptionsPopup : Popup
{
    private Button? _selectedWorkoutTypeButton;
    private Button? _selectedEquipmentButton;

    public WorkoutOptionsPopup()
    {
        InitializeComponent();
    }

    private void OnCancel_Clicked(object sender, EventArgs e)
    {
        Close();
    }

    private void OnWorkoutTypeClicked(object sender, EventArgs e)
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

    private void OnNext_Clicked(object sender, EventArgs e)
    {
        if (_selectedWorkoutTypeButton == null)
        {
            Application.Current?.MainPage?.DisplayAlert(
                "Missing selection",
                "Please choose your workout type.",
                "OK");
            return;
        }

        var workoutType = _selectedWorkoutTypeButton.Text;

        if (workoutType == "Cardio")
        {
            // For cardio, skip equipment selection and submit directly
            var result = new EquipmentResult
            {
                WorkoutType = "Cardio",
                HomeAccess = false,
                GymAccess = false // Equipment doesn't matter for cardio
            };
            Close(result);
        }
        else
        {
            // For strength training, show equipment selection
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

    private void OnSubmit_Clicked(object sender, EventArgs e)
    {
        if (_selectedEquipmentButton == null)
        {
            Application.Current?.MainPage?.DisplayAlert(
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

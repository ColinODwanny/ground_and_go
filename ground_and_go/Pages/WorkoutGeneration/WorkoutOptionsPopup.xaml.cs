// Samuel Reynebeau
using CommunityToolkit.Maui.Views;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class WorkoutOptionsPopup : Popup
{
    private Button _selectedEquipmentButton;

    public WorkoutOptionsPopup()
    {
        InitializeComponent();
    }

    private void OnCancel_Clicked(object sender, EventArgs e)
    {
        Close();
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
            Application.Current.MainPage.DisplayAlert(
                "Missing selection",
                "Please choose your equipment.",
                "OK");
            return;
        }

        var text = _selectedEquipmentButton.Text;

        var result = new EquipmentResult
        {
            HomeAccess = text == "Home Equipment",
            GymAccess = text == "Gym Equipment"
        };
        
        Close(result);
    }
}

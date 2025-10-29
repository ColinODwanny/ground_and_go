// Devlin Delegard
using CommunityToolkit.Maui.Views;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class WorkoutOptionsPopup : Popup
{
    public WorkoutOptionsPopup()
    {
        InitializeComponent();
    }

    // submit
    private async void OnSubmit_Clicked(object sender, EventArgs e)
    {
        var result = new EquipmentResult
        {
            HomeAccess = HomeCheckbox.IsChecked,
            GymAccess = GymCheckbox.IsChecked,
        };
        Close(result); //The workout page will use these results + the inputted feelings to generate a workout
    }
}
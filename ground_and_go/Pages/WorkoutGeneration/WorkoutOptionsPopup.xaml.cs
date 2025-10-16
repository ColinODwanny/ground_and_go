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
        //TODO: This will be implemented to store checkbox results
        Close();
    }
}
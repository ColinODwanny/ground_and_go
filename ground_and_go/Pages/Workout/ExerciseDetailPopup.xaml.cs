//Aidan Trusky
using System.Collections;
using CommunityToolkit.Maui.Views;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class ExerciseDetailPopup : Popup
{
    public string ExerciseName { get; set; }
    
    public ExerciseDetailPopup(string exerciseName)
    {
        InitializeComponent();
        ExerciseName = exerciseName;
        ExerciseTitle.Text = exerciseName;
    }

    private void OnComplete_Clicked(object sender, EventArgs e)
    {
        Close(ExerciseName);
    }

    private async void OnViewExerciseClicked(object sender, EventArgs e)
{
        await Application.Current.MainPage.Navigation.PushAsync(new ground_and_go.Pages.Workout.VideoPlayer());
        Close();
}

}
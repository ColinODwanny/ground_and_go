//Aidan Trusky
using System.Collections;
using CommunityToolkit.Maui.Views;
using ground_and_go.Models;

namespace ground_and_go.Pages.WorkoutGeneration;

public partial class ExerciseDetailPopup : Popup
{
    public string ExerciseName { get; set; }
    public WorkoutExerciseItem? ExerciseData { get; set; }
    
    public ExerciseDetailPopup(string exerciseName, WorkoutExerciseItem? exerciseData = null)
    {
        InitializeComponent();
        ExerciseName = exerciseName;
        ExerciseData = exerciseData;
        ExerciseTitle.Text = exerciseName;
        
        // Add exercise details and notes if available
        if (exerciseData != null)
        {
            AddExerciseDetails(exerciseData);
        }
    }
    
    private void AddExerciseDetails(WorkoutExerciseItem exercise)
    {
        var mainLayout = (VerticalStackLayout)((Border)Content!).Content!;
        
        // Find the position to insert details (after title, before buttons)
        int insertIndex = 1;
        
        // Add exercise details (excluding rest)
        var details = new List<string>();
        if (!string.IsNullOrEmpty(exercise.SetsDisplay))
            details.Add($"Sets: {exercise.SetsDisplay}");
        if (!string.IsNullOrEmpty(exercise.Reps))
            details.Add($"Reps: {exercise.Reps}");
        if (!string.IsNullOrEmpty(exercise.Duration))
            details.Add($"Duration: {exercise.Duration}");
            
        if (details.Count > 0)
        {
            var detailsLabel = new Label
            {
                Text = string.Join("\n", details),
                FontSize = 15,
                TextColor = Colors.DarkGray,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            mainLayout.Children.Insert(insertIndex++, detailsLabel);
        }

        // Add rest as a separate section if available
        if (!string.IsNullOrEmpty(exercise.Rest))
        {
            var restHeaderLabel = new Label
            {
                Text = "Rest Period:",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 2)
            };
            mainLayout.Children.Insert(insertIndex++, restHeaderLabel);
            
            var restLabel = new Label
            {
                Text = exercise.Rest,
                FontSize = 15,
                TextColor = Colors.DarkGray,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            mainLayout.Children.Insert(insertIndex++, restLabel);
        }
        
        // Add notes if available
        if (!string.IsNullOrEmpty(exercise.Note))
        {
            var notesLabel = new Label
            {
                Text = "Instructions:",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5, 0, 2)
            };
            mainLayout.Children.Insert(insertIndex++, notesLabel);
            
            var noteText = new Label
            {
                Text = exercise.Note,
                FontSize = 14,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            mainLayout.Children.Insert(insertIndex++, noteText);
        }
    }

    private void OnComplete_Clicked(object sender, EventArgs e)
    {
        Close(ExerciseName);
    }

    private async void OnViewExerciseClicked(object sender, EventArgs e)
{
        // use shell navigation with the registered route and pass exercise name
        await Shell.Current.GoToAsync($"{nameof(ground_and_go.Pages.Workout.VideoPlayer)}?exerciseName={Uri.EscapeDataString(ExerciseName)}");
        Close();
}

}
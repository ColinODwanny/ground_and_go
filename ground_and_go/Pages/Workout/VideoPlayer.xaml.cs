//Colin O'Dwanny
using System;
using ground_and_go.Models;

namespace ground_and_go.Pages.Workout
{
    [QueryProperty(nameof(ExerciseName), "exerciseName")]
    public partial class VideoPlayer : ContentPage
    {
        private Database database;
        public string ExerciseName { get; set; } = string.Empty;
        
        public VideoPlayer()
        {
            InitializeComponent();
            database = new Database();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadExerciseVideo();
        }

        private async Task LoadExerciseVideo()
        {
            if (string.IsNullOrEmpty(ExerciseName))
            {
                VideoWebView.Source = "https://www.youtube.com/shorts/hWbUlkb5Ms4";
                return;
            }

            try
            {
                await database.EnsureInitializedAsync();
                var exercises = await database.GetAllExercises();
                
                var exercise = exercises.Values.FirstOrDefault(e => 
                    string.Equals(e.Name, ExerciseName, StringComparison.OrdinalIgnoreCase));
                
                if (exercise != null && !string.IsNullOrEmpty(exercise.VideoLink))
                {
                    VideoWebView.Source = exercise.VideoLink;
                }
                else
                {
                    VideoWebView.Source = "https://www.youtube.com/shorts/hWbUlkb5Ms4";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading exercise video: {ex.Message}");
                VideoWebView.Source = "https://www.youtube.com/shorts/hWbUlkb5Ms4";
            }
        }

        //Stops video when you press the back button
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            VideoWebView.Source = "about:blank";
        }
    }
}
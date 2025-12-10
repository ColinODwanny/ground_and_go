//Colin O'Dwanny
using System;
using ground_and_go.Models;
using ground_and_go.Services;

namespace ground_and_go.Pages.Workout
{
    [QueryProperty(nameof(ExerciseName), "exerciseName")]
    public partial class VideoPlayer : ContentPage
    {
        private readonly Database _database;
        private readonly ConfigurationService _configService;
        public string ExerciseName { get; set; } = string.Empty;
        
        public VideoPlayer(Database database, ConfigurationService configService)
        {
            InitializeComponent();
            _database = database;
            _configService = configService;
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
                VideoWebView.Source = _configService.GetDefaultVideoUrl();
                return;
            }

            try
            {
                await _database.EnsureInitializedAsync();
                var exercises = await _database.GetAllExercises();
                
                var exercise = exercises.Values.FirstOrDefault(e => 
                    string.Equals(e.Name, ExerciseName, StringComparison.OrdinalIgnoreCase));
                
                if (exercise != null && !string.IsNullOrEmpty(exercise.VideoLink))
                {
                    VideoWebView.Source = exercise.VideoLink;
                }
                else
                {
                    VideoWebView.Source = _configService.GetDefaultVideoUrl();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading exercise video: {ex.Message}");
                VideoWebView.Source = _configService.GetDefaultVideoUrl();
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
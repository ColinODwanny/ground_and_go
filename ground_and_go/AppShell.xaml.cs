namespace ground_and_go;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // register routes for all pages that we navigate to
        // these are pages we push onto the navigation stack
        Routing.RegisterRoute(nameof(Pages.Profile.MyWorkoutsPage), typeof(Pages.Profile.MyWorkoutsPage));
        Routing.RegisterRoute(nameof(Pages.Profile.MyJournalEntriesPage), typeof(Pages.Profile.MyJournalEntriesPage));
        Routing.RegisterRoute(nameof(Pages.Profile.WorkoutPage), typeof(Pages.Profile.WorkoutPage));
        Routing.RegisterRoute(nameof(Pages.Workout.VideoPlayer), typeof(Pages.Workout.VideoPlayer));
        
        // need unique routes for pages with the same class name
        Routing.RegisterRoute("ProfileJournalEntry", typeof(ground_and_go.Pages.Profile.JournalEntryPage));
        Routing.RegisterRoute("WorkoutJournalEntry", typeof(ground_and_go.Pages.WorkoutGeneration.JournalEntryPage));

        // add these routes for the mindfulness pages
        Routing.RegisterRoute(nameof(Pages.WorkoutGeneration.MindfulnessActivityWorkoutPage), typeof(Pages.WorkoutGeneration.MindfulnessActivityWorkoutPage));
        Routing.RegisterRoute(nameof(Pages.WorkoutGeneration.MindfulnessActivityRestPage), typeof(Pages.WorkoutGeneration.MindfulnessActivityRestPage));

        // add the new post-activity journal page route
        Routing.RegisterRoute(nameof(Pages.WorkoutGeneration.PostActivityJournalEntryPage), typeof(Pages.WorkoutGeneration.PostActivityJournalEntryPage));
    }
}
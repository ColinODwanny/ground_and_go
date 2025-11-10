using Microsoft.Extensions.Logging;
using MauiIcons.Material;
using CommunityToolkit.Maui;
using ground_and_go.Services;  
using ground_and_go.Pages.Home; 
using ground_and_go.Pages.WorkoutGeneration; 
using ground_and_go.Pages.Workout; 

namespace ground_and_go;

public static class MauiProgram
{
    public static Database? db { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // use these packages
        builder
            .UseMauiApp<App>()
            .UseMaterialMauiIcons()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- Service Registrations ---

        // This creates the *one and only* instance of the Database
        builder.Services.AddSingleton<Database>();
        
        // Register our new services
        builder.Services.AddSingleton<ground_and_go.Services.MockAuthService>();
        builder.Services.AddSingleton<ground_and_go.Services.DailyProgressService>();

        // Register pages that need services
        builder.Services.AddTransient<ground_and_go.Pages.Home.HomePage>(); 
        builder.Services.AddTransient<ground_and_go.Pages.Profile.MyWorkoutsPage>();
        builder.Services.AddTransient<ground_and_go.Pages.Profile.MyJournalEntriesPage>();
        
        // --- Register ALL pages in the workout/rest flow ---
        builder.Services.AddTransient<ground_and_go.Pages.WorkoutGeneration.JournalEntryPage>();
        builder.Services.AddTransient<ground_and_go.Pages.WorkoutGeneration.MindfulnessActivityWorkoutPage>(); 
        builder.Services.AddTransient<ground_and_go.Pages.WorkoutGeneration.MindfulnessActivityRestPage>();
        builder.Services.AddTransient<ground_and_go.Pages.WorkoutGeneration.TodaysWorkoutPage>(); 
        builder.Services.AddTransient<ground_and_go.Pages.WorkoutGeneration.PostActivityJournalEntryPage>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        // --- Build the App ---
        var app = builder.Build();

        // Get the singleton Database instance from the service provider...
        // ...and assign it to our 'db' property.
        db = app.Services.GetRequiredService<Database>()!;

        return app;
    }
}
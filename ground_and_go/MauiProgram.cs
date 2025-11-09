using Microsoft.Extensions.Logging;
using MauiIcons.Material;
using CommunityToolkit.Maui;
using ground_and_go.Services;  // <-- Make sure Services are imported
using ground_and_go.Pages.Home; // <-- Make sure Home page is imported

namespace ground_and_go;

public static class MauiProgram
{
    // CHANGE #1:
    // Made the property nullable with a '?' to fix the CS8618 error.
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
        builder.Services.AddTransient<ground_and_go.Pages.WorkoutGeneration.JournalEntryPage>();
        // We will need to register the other pages in the flow here too
        // (JournalEntryPage, MindfulnessActivityWorkoutPage, etc.)
        // But we can do that in the next steps.

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // --- Build the App ---
        var app = builder.Build();

        // CHANGE #2:
        // Get the singleton Database instance from the service provider...
        // ...and assign it to our 'db' property.
        // We add '!' at the end to tell the compiler we know this won't be null.
        db = app.Services.GetRequiredService<Database>()!;

        return app;
    }
}
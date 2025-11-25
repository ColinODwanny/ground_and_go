using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ground_and_go;

public partial class App : Application
{
    private readonly ILogger<App>? _logger;

    public App()
    {
        InitializeComponent();
        
        // Get logger service if available
        try
        {
            _logger = IPlatformApplication.Current?.Services?.GetService<ILogger<App>>();
        }
        catch
        {
            // Logger not available yet during construction
        }
        
        // Multiple logging approaches for debugging
        Console.WriteLine("✓ Ground & Go App Constructor - Debug logging test");
        Debug.WriteLine("✓ Ground & Go App Constructor - Debug.WriteLine test");
        System.Diagnostics.Trace.WriteLine("✓ Ground & Go App Constructor - Trace.WriteLine test");
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Console.WriteLine("✓ Creating main window...");
        _logger?.LogInformation("✓ Creating main window...");
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        Console.WriteLine("✓ App OnStart() called - Initializing database services...");
        Debug.WriteLine("✓ App OnStart() called - Initializing database services...");
        _logger?.LogInformation("✓ App OnStart() called - Initializing database services...");

        var db = IPlatformApplication.Current.Services.GetService<Database>();

        if (db is null)
        {
            var errorMsg = "✗ Database service not found during OnStart()";
            Console.WriteLine(errorMsg);
            Debug.WriteLine(errorMsg);
            _logger?.LogError(errorMsg);
            return;
        }

        Console.WriteLine("✓ Database service found, initializing...");
        _logger?.LogInformation("✓ Database service found, initializing...");
        
        await db.EnsureInitializedAsync();
        await db.LoadExercises();
        await db.LoadMindfulness();
        
        Console.WriteLine("✓ Database initialization completed successfully");
        _logger?.LogInformation("✓ Database initialization completed successfully");
    }
}

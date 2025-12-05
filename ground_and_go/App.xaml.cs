using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace ground_and_go;

public partial class App : Application
{
    private readonly ILogger<App>? _logger;

    public App()
    {
        InitializeComponent();

        // force light theme
        UserAppTheme = AppTheme.Light;

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

    protected override async void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var type = queryParams["type"];
        var token = queryParams["token"];
        var refreshToken = queryParams["refresh_token"];


        if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
        {
            // Missing parameters - show error or navigate to a safe page
            await Shell.Current.DisplayAlert("Error", "Invalid link. Missing required information.", "OK");
            await Shell.Current.GoToAsync("//login");
            return;
        }

        try
        {
            if (type == "recovery")
            {

                var db = IPlatformApplication.Current!.Services.GetService<Database>();
                db?.SetSupabaseSession(token, refreshToken);

                // Navigate to Reset Password page
                await Shell.Current.GoToAsync("//forgotpassword");
            }
            else
            {
                // Navigate to login page
                await Shell.Current.GoToAsync("//login");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
        }
    }


    protected override Window CreateWindow(IActivationState? activationState)
    {
        Console.WriteLine("✓ Creating main window...");
        _logger?.LogInformation("✓ Creating main window...");
        var db = IPlatformApplication.Current!.Services.GetService<Database>();
        var window = new Window(new AppShell());

        window.Dispatcher.Dispatch(async () =>
        {
            var sessionJson = await SecureStorage.GetAsync("login_session"); //Takes the user's login token
            if (sessionJson != null)
            {
                var session = JsonSerializer.Deserialize<Supabase.Gotrue.Session>(sessionJson);
                if (session != null)
                {
                    db!.SetSupabaseSession(session.AccessToken!, session.RefreshToken!); //Starts a session with Supabase

                    if (session.Expired())
                    {
                        db!.refreshSupabaseSession();
                    }

                    await Shell.Current.GoToAsync("//home");
                    return;
                }
            }

            await Shell.Current.GoToAsync("//login");
        });
        return window;
    }

    protected override async void OnStart()
    {
        Console.WriteLine("✓ App OnStart() called - Initializing database services...");
        Debug.WriteLine("✓ App OnStart() called - Initializing database services...");
        _logger?.LogInformation("✓ App OnStart() called - Initializing database services...");

        var db = IPlatformApplication.Current!.Services.GetService<Database>();

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

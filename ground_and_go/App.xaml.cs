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
                if (db != null)
                {
                    await db.SetSupabaseSession(token, refreshToken);
                }

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
            try
            {
                Console.WriteLine("Checking for saved session...");
                var sessionJson = await SecureStorage.GetAsync("login_session");
                
                Console.WriteLine($"SecureStorage result: {(string.IsNullOrEmpty(sessionJson) ? "EMPTY/NULL" : $"Found {sessionJson.Length} characters")}");
                
                // If SecureStorage is empty, check fallback storage
                if (string.IsNullOrEmpty(sessionJson))
                {
                    sessionJson = Preferences.Get("login_session_fallback", "");
                    Console.WriteLine($"Fallback storage result: {(string.IsNullOrEmpty(sessionJson) ? "EMPTY/NULL" : $"Found {sessionJson.Length} characters")}");
                }
                
                if (!string.IsNullOrEmpty(sessionJson))
                {
                    Console.WriteLine("Found saved session, attempting to restore...");
                    Console.WriteLine($"Session JSON (first 100 chars): {sessionJson.Substring(0, Math.Min(100, sessionJson.Length))}...");
                    
                    var session = JsonSerializer.Deserialize<Supabase.Gotrue.Session>(sessionJson);
                    Console.WriteLine($"Deserialized session: {(session != null ? "SUCCESS" : "FAILED")}");
                    
                    if (session != null && !string.IsNullOrEmpty(session.AccessToken) && !string.IsNullOrEmpty(session.RefreshToken))
                    {
                        await db!.EnsureInitializedAsync();
                        var sessionRestored = await db.SetSupabaseSession(session.AccessToken, session.RefreshToken);
                        
                        if (sessionRestored && db.HasValidSession())
                        {
                            Console.WriteLine("Session restored successfully, navigating to home");
                            await Shell.Current.GoToAsync("//home");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Session restoration failed, clearing stored session");
                            await SecureStorage.SetAsync("login_session", "");
                        }
                    }
                }
                
                Console.WriteLine("No valid session found, navigating to login");
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during session restoration: {ex.Message}");
                await Shell.Current.GoToAsync("//login");
            }
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

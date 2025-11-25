using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace ground_and_go;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override async void OnAppLinkRequestReceived(Uri uri)
    {
        base.OnAppLinkRequestReceived(uri);

        var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var action = queryParams["action"];
        var token = queryParams["token"];


        if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(token))
        {
            // Missing parameters - show error or navigate to a safe page
            await Shell.Current.DisplayAlert("Error", "Invalid link. Missing required information.", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }
        await HandleEmailConfirmation(action, token);
    }


    private async Task HandleEmailConfirmation(string action, string token)
    {
        try
        {
            // Calls backend API to confirm the email
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(
                "https://irekjohmgsjicpszbgus.supabase.co/auth/v1/verify",
                new StringContent($"{{\"token\":\"{token}\",\"type\":\"{action}\"}}", Encoding.UTF8, "application/json")
            );

            if (response.IsSuccessStatusCode)
            {
                if (action == "recovery")
                {
                    // Success - Navigate to Reset Password page
                    await Shell.Current.GoToAsync("//LoginPage"); //TODO Replace with new page
                }
                else
                {
                    // Success - Navigate to login page
                    await Shell.Current.GoToAsync("//LoginPage");
                }
            }
            else
            {
                // Show error
                await Shell.Current.DisplayAlert("Error", "Invalid or expired token.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Something went wrong: {ex.Message}", "OK");
        }
    }


    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        var db = IPlatformApplication.Current.Services.GetService<Database>();

        if (db is null)
        {
            Console.WriteLine("Database service not found.");
            return;
        }

        await db.EnsureInitializedAsync();
        await db.LoadExercises();
        await db.LoadMindfulness();
    }
}

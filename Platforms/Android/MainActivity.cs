using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace ground_and_go;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, Exported = true)]
[IntentFilter(new[] {Intent.ActionView}, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "groundandgo", DataHost = "auth-callback")]
public class MainActivity : MauiAppCompatActivity
{

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        HandleIntent(Intent); // Handle cold start deep link
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        HandleIntent(intent); // Handle deep link when app is already running
    }

    private void HandleIntent(Intent? intent)
    {
        var data = intent?.DataString; // This will be the URL, such as groundandgo://auth-callback?token=XYZ&type=recovery
        if (!string.IsNullOrEmpty(data))
        {
            try
            {
                var uri = new Uri(data);
                Microsoft.Maui.Controls.Application.Current?.SendOnAppLinkRequestReceived(uri);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Deep link error: {ex}");
            }
        }
    }

}

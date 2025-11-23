using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace ground_and_go;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
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

using Microsoft.Extensions.Logging;
using MauiIcons.Material;
using CommunityToolkit.Maui;

namespace ground_and_go;

public static class MauiProgram
{
    public static Database db = new Database();
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

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
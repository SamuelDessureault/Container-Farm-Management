using ContainerFarmManagement.Views;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Maui.GoogleMaps.Hosting;
using ContainerFarmManagement.Views.FarmOwnerViews;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.LifecycleEvents;
using ContainerFarmManagement.Services;

namespace ContainerFarmManagement;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        var a = Assembly.GetExecutingAssembly();
        using var stream = a.GetManifestResourceStream("ContainerFarmManagement.appsettings.json");
        var config = new ConfigurationBuilder()
        .AddJsonStream(stream)
                    .Build();
        builder.Configuration.AddConfiguration(config);

        builder
            .UseMauiApp<App>()
#if ANDROID
                .UseGoogleMaps()
#elif IOS
                .UseGoogleMaps("AIzaSyDpyRwbSM3XZUckVijWOku3PovAJS3cVlo")
#endif
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureLifecycleEvents(events =>
            {

                events.AddAndroid(android => android //needs to be configured for each platform
                    .OnResume(activity => OnStart())
                    .OnPause(activity => OnStop()));
                events.AddEvent("Activated", OnStart);
                events.AddEvent("Deactivated", OnStop);

                static void OnStart()
                {
                    try
                    {
                        App.AzureService.StartReading();
                    }
                    catch (Exception ex)
                    {

                    }
                    return;
                }
                static bool OnStop()
                {
                    try
                    {
                        App.AzureService.CancelReading();
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    return true;
                }
            }
                );

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<ContainerListPage>();
        builder.Services.AddSingleton<GeoLocationView>();
        builder.Services.AddSingleton<OwnerHistoricalData>();
        builder.Services.AddSingleton<AddEditOwner>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        var app = builder.Build();

        Services = app.Services;
        return app;
    }
    //Service Property need to access the services in the app 
    public static IServiceProvider Services { get; private set; }
}



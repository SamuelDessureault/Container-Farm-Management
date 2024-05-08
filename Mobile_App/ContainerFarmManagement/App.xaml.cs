using ContainerFarmManagement.Config;
using ContainerFarmManagement.Models;
using ContainerFarmManagement.Repos;
using ContainerFarmManagement.Services;
using Microsoft.Extensions.Configuration;
using System.Timers;

namespace ContainerFarmManagement;

public partial class App : Application
{
    private static ReadingRepo readingRepo;
    private static ContainerRepo containerRepo;
    private static AccountRepo accountRepo;
    private static DataRepo dataRepo;
    private static System.Timers.Timer timer;//will be used to read the event hub at set intervals
    private static AzureService azureService;

    public static Account Account { get; set; }


    private static void SetTimer()
    {
        // Create a timer with a two second interval.
        timer = new System.Timers.Timer(ResourceStrings.Interval);
        // Hook up the Elapsed event for the timer. 
        timer.Elapsed += OnTimedEvent;
        timer.AutoReset = false;//will only run once
        timer.Enabled = true;
    }

    private static void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
        //This method will be executed every interval set within the SetTimer method
        //azureService.ReadFromEventHub();
    }

    public static bool IsTest()
    {
        return true;
    }

    public static DataRepo DataRepo
    {
        get { return dataRepo ??= new DataRepo(); }
    }
    public static ReadingRepo ReadingRepository
    {
        get { return readingRepo ??= new ReadingRepo(); }
    }
    public static ContainerRepo ContainerRepo
    {
        get { return containerRepo ??= new ContainerRepo(); }
    }

    public static AzureService AzureService
    {
        get { return azureService ??= new AzureService(); }
    }

    public static AccountRepo AccountRepo
    {
        get { return accountRepo ??= new AccountRepo(); }
    }
    public static Settings Settings { get; private set; }
        = MauiProgram.Services.GetService<IConfiguration>()
            .GetRequiredSection(nameof(Settings)).Get<Settings>();

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

}
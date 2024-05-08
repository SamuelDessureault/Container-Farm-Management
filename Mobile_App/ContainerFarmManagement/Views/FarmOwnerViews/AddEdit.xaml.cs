using Android.OS;
using ContainerFarmManagement.Config;
using ContainerFarmManagement.Models;
using ContainerFarmManagement.Services;
using Microsoft.Azure.Devices;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ContainerFarmManagement.Views.FarmOwnerViews;

public partial class AddEditOwner : ContentPage
{

    private Container Container { get; set; }

    private bool isNewContainer;
    private const int DEFAULT_LOW_THRESHOLD = 5;
    private const int DEFAULT_HIGH_THRESHOLD = 25;
    private const int DEFAULT_TELEMETRY_INTERVAL = 5;

    private ObservableCollection<Container> Containers { get; set; }


    public AddEditOwner(Container container = null)
    {
        InitializeComponent();

        System.Diagnostics.Debug.Print($"{App.ContainerRepo.Containers.Count}");

        Containers = App.ContainerRepo.Containers;
        Container = container != null ? container : new Container();

        if (container == null)
        {
            Title = "Add Container";
            isNewContainer = true;
            containerNumber.Text = (Containers.Count + 1).ToString();
        }
        else
        {

            Title = "Edit " + Container.Name;
            isNewContainer = false;
            containerNumber.Text = (Container.Key).ToString();

        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        BindingContext = Container;

    }

    private async void Button_Clicked(object sender, EventArgs e)
    {

        try
        {
            if (containerLabel.Text == null)
            {
                await DisplayAlert("Empty Label", "Please enter a container label", "OK");
                return;
            }

            if (isNewContainer)
            {
                bool registerdDevice = await AzureService.RegisterDevice(containerDeviceEntry.Text);

                if (registerdDevice)
                {
                    ServiceClient client = ServiceClient.CreateFromConnectionString(App.Settings.HubConnectionString);
                    Container = new Container(containerLabel.Text, client, containerDeviceEntry.Text);
                    Container.RegisteredUsers.Add(App.Account.Key);
                    await App.ContainerRepo.AddContainer(Container);
                }
                else
                {
                    await DisplayAlert("ERROR", "Error: Could not register device", "OK");
                    return;
                }
            }
            else
            {
                Container.Name = containerLabel.Text;
            }
            await UpdateTwinProperties();
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            // Logging the exception or showing an alert with error details can be done here
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async Task UpdateTwinProperties()
    {
        await AzureService.SetDesiredDeviceProperty(Container.DeviceId, ResourceStrings.LowTemperatureThresholdPropertyName, DEFAULT_LOW_THRESHOLD);
        await AzureService.SetDesiredDeviceProperty(Container.DeviceId, ResourceStrings.HighTemperatureThresholdPropertyName, DEFAULT_HIGH_THRESHOLD);
        await AzureService.SetDesiredDeviceProperty(Container.DeviceId, ResourceStrings.TelemetryIntervalPropertyName, DEFAULT_TELEMETRY_INTERVAL);
    }




}
using ContainerFarmManagement.Models;
using ContainerFarmManagement.Models.OwnerViewReadings;
using ContainerFarmManagement.Services;
using ContainerFarmManagement.Utilities;
using Maui.GoogleMaps;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ContainerFarmManagement.Views.FarmOwnerViews;

public partial class GeoLocationView : ContentPage
{

    private Models.Container container { get; set; }
    private Pin Pin { get; set; }

    //public ObservableCollection<GeoReading> GeoReadings { get; set; } = new ObservableCollection<GeoReading>();


    public GeoLocationView(Models.Container _container)
    {
        InitializeComponent();
        container = _container;

        Title = container.Name;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();



        await GetCoordinatesAndSetPin();
        //await GetReadings();

        BindingContext = container.GeoLocationSubsystem;
    }

    private async Task GetCoordinatesAndSetPin()
    {
        Reading longitude = await container.GeoLocationSubsystem.GetLatest(Reading.SensorTypes.GPS, Reading.Units.DEGREES_LONGITUDE);
        Reading latitude = await container.GeoLocationSubsystem.GetLatest(Reading.SensorTypes.GPS, Reading.Units.DEGREES_LATITUDE);

        if (latitude != null && longitude != null)
        {

            if (Pin != null)
            {
                containerMap.Pins.Remove(Pin);

                // Check if the new coordinates are different, and if so, display an alert
                if (Pin.Position.Latitude != latitude.Value || Pin.Position.Longitude != longitude.Value)
                {
                    await DisplayAlert("Location Changed", "The container has moved to a new location.", "OK");
                }
            }


            Pin = new Pin()
            {
                Label = container.Name,
                Address = "123 Farm Street",
                Position = new Position(latitude.Value, longitude.Value),
                Type = PinType.Place,
                IsVisible = true,
            };
            containerMap.Pins.Add(Pin);
        }
        else
        {
            await DisplayAlert("No Location", "No location was found", "ok");
        }
    }


    public async Task RefreshLocation()
    {
        await GetCoordinatesAndSetPin();
    }

    private async void Alarm_Toggled(object sender, ToggledEventArgs e)
    {
        Microsoft.Maui.Controls.Switch toggle = (Microsoft.Maui.Controls.Switch)sender;
        Dictionary<string, object> values = new Dictionary<string, object>();
        float value = toggle.IsToggled ? 1 : 0;
        values["value"] = value;
        Models.Command cmd = new Models.Command(Models.Command.ActuatorTypes.BUZZER, values);
        int response = await container.SecuritySubsystem.ControlActuator(cmd);
        if (response == 500)
        {
            await DisplayAlert("Error 500", "Internal server error when communicating with the buzzer.", "OK");
        }
        else if (response == -1)
        {
            await DisplayAlert("Error", "There was an error communicating with the buzzer.", "OK");
        }
        await UpdateAlarm();
    }

    private async void doorLockToggle_Toggled(object sender, ToggledEventArgs e)
    {
        Microsoft.Maui.Controls.Switch toggle = (Microsoft.Maui.Controls.Switch)sender;
        Dictionary<string, object> values = new Dictionary<string, object>();
        float value = toggle.IsToggled ? 1 : 0;
        values["value"] = value;
        Models.Command cmd = new Models.Command(Models.Command.ActuatorTypes.LOCK, values);
        int response = await AzureService.InvokeMethod(cmd, container.DeviceId);
        if (response == 500)
        {
            await DisplayAlert("Error 500", "Internal server error when communicating with the lock.", "OK");
        }
        else if (response == -1)
        {
            await DisplayAlert("Error", "There was an error communicating with the lock.", "OK");
        }
        await UpdateLock();
    }


    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.FarmOwnerViews.OwnerHistoricalData(container));
    }

    private async Task UpdateAlarm()
    {
        try
        {
            TwinProperties twinPropertiesBuzzer = await AzureService.GetDeviceProperties(container.DeviceId);
            var buzzerState = twinPropertiesBuzzer.Reported[Models.Command.ActuatorTypes.BUZZER.Description()];

            Alarm.IsToggled = buzzerState["value"];



        }
        catch (Exception)
        {
            await DisplayAlert("Error", "There was an issue accessing the device twin.", "OK");
        }
    }

    private async Task UpdateLock()
    {
        try
        {
            TwinProperties twinPropertiesDoorLock = await AzureService.GetDeviceProperties(container.DeviceId);
            var doorLockState = twinPropertiesDoorLock.Reported[Models.Command.ActuatorTypes.LOCK.Description()];

            doorLockToggle.IsToggled = doorLockState["value"];
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "There was an issue accessing the device twin.", "OK");
        }
    }
}
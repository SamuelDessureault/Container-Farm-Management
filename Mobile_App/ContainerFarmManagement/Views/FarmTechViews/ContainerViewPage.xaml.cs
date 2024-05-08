using ContainerFarmManagement.Models;
using ContainerFarmManagement.Repos;
using ContainerFarmManagement.Services;
using ContainerFarmManagement.Utilities;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Maui.Controls;
using System.Linq;

namespace ContainerFarmManagement.Views.FarmTechViews;

public partial class ContainerViewPage : ContentPage
{
    public List<ISeries> Series { get; set; }
    public Container Container { get; set; }
    public Axis[] XAxis { get; set; }
    public Axis[] YAxis { get; set; }

    public ContainerViewPage(Container container)
    {
        InitializeComponent();
        Container = container;
        Title = Container.Name;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await UpdateToggles();
        await Container.PlantSubsystem.UpdateData();
        BindingContext = Container.PlantSubsystem;
    }

    private async Task UpdateToggles()
    {
        TwinProperties twinProperties = null;
        try
        {
            twinProperties = await AzureService.GetDeviceProperties(Container.DeviceId);
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "There was an issue accessing the device twin.", "OK");
        }
        try
        {
            var fanState = twinProperties?.Reported[Models.Command.ActuatorTypes.FAN.Description()];

            fanToggle.IsToggled = fanState["value"];
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "There was an issue accessing the fan from the device twin.", "OK");
        }
        try
        {
            string led_index = "1";
            var rgbState = twinProperties?.Reported[Models.Command.ActuatorTypes.RGB.Description()];

            lightToggle.IsToggled = rgbState["led_states"][led_index];
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "There was an issue accessing the light from the device twin.", "OK");
        }
        try
        {
            var lockState = twinProperties?.Reported[Models.Command.ActuatorTypes.LOCK.Description()];

            lightToggle.IsToggled = lockState["value"];
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "There was an issue accessing the lock from the device twin.", "OK");
        }
    }

    private async void toolEdit_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.FarmTechViews.AddEditContainerPage(Container));
    }

    private async void fanToggle_Toggled(object sender, ToggledEventArgs e)
    {
        Switch toggle = (Switch)sender;
        Dictionary<string, object> values = new Dictionary<string, object>();
        float value = toggle.IsToggled ? 1 : 0;
        values["value"] = value;
        Models.Command cmd = new Models.Command(Models.Command.ActuatorTypes.FAN, values);

        int response = await Container.PlantSubsystem.ControlActuator(cmd);
        if (response == 500)
        {
            await DisplayAlert("Error 500", "Internal server error when communicating with the fan.", "OK");
        }
        else if (response == 404)
        {
            await DisplayAlert("Error 404", "The wrong values for the fan were inputed.", "OK");
        }
        else if (response == 403)
        {
            await DisplayAlert("Error 403", "You do not have permission to use the fan.", "OK");
        }
        else if (response == -1)
        {
            await DisplayAlert("Error", "There was an error communicating with the fan.", "OK");
        }
    }

    private async void lightToggle_Toggled(object sender, ToggledEventArgs e)
    {
        Switch toggle = (Switch)sender;
        Dictionary<string, object> values = new Dictionary<string, object>();
        float value = toggle.IsToggled ? 1 : 0;
        if (toggle.IsToggled)
        {
            values["led_index"] = 1;
            values["color"] = new[] { 255, 0, 0 };
        }
        else
        {
            values["led_index"] = 1;
            values["color"] = new[] { 0, 0, 0 };
        }
        Models.Command cmd = new Models.Command(Models.Command.ActuatorTypes.RGB, values);

        int response = await Container.PlantSubsystem.ControlActuator(cmd);
        if (response == 500)
        {
            await DisplayAlert("Error 500", "Internal server error when communicating with the light.", "OK");
        }
        else if (response == 404)
        {
            await DisplayAlert("Error 404", "The wrong values for the light were inputed.", "OK");
        }
        else if (response == 403)
        {
            await DisplayAlert("Error 403", "You do not have permission to use the light.", "OK");
        }
        else if (response == -1)
        {
            await DisplayAlert("Error", "There was an error communicating with the light.", "OK");
        }
    }
    private async void lockToggle_Toggled(object sender, ToggledEventArgs e)
    {
        Switch toggle = (Switch)sender;
        Dictionary<string, object> values = new Dictionary<string, object>();
        float value = toggle.IsToggled ? 1 : 0;
        values["value"] = value;
        Models.Command cmd = new Models.Command(Models.Command.ActuatorTypes.LOCK, values);

        int response = await Container.PlantSubsystem.ControlActuator(cmd);
        if (response == 500)
        {
            await DisplayAlert("Error 500", "Internal server error when communicating with the lock.", "OK");
        }
        else if (response == 404)
        {
            await DisplayAlert("Error 404", "The wrong values for the lock were inputed.", "OK");
        }
        else if (response == 403)
        {
            await DisplayAlert("Error 403", "You do not have permission to use the lock.", "OK");
        }
        else if (response == -1)
        {
            await DisplayAlert("Error", "There was an error communicating with the lock.", "OK");
        }

    }

    private async void historyDataBtn_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new FarmTechViews.ContainerHistoryPage(Container));
    }


}
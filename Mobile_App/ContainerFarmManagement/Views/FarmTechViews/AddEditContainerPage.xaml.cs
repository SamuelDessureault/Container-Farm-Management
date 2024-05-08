using ContainerFarmManagement.Config;
using ContainerFarmManagement.Models;
using ContainerFarmManagement.Services;
using Microsoft.Maui.Controls;

namespace ContainerFarmManagement.Views.FarmTechViews;

public partial class AddEditContainerPage : ContentPage
{
	private Container _container;

    /// <summary>
    /// True if add, false if edit.
    /// </summary>
    private bool _isAddOrEdit;

    public AddEditContainerPage(Container container = null)
	{
		InitializeComponent();
        _container = container != null ? container : new Container();

        if (container == null)
        {
            Title = "Add Container";
            _isAddOrEdit = true;
            containerIDFrame.IsVisible = true;
        }
        else
        {
            Title = "Edit " + _container.Name;
            _isAddOrEdit = false;
            containerIDFrame.IsVisible = false;
        }
        containerIDPicker.ItemsSource = App.ContainerRepo.Containers.Where(c => !c.RegisteredUsers.Contains(App.Account.Key)).Select(c => c.Key).ToList();

        BindingContext = _container;
    }

    private async void confirmBtn_Clicked(object sender, EventArgs e)
    {
        if (_isAddOrEdit)
        {
            try
            {
                Container containerToCopy = await App.ContainerRepo.GetContainer(_container.Key);
                _container = new Container(containerToCopy);
                _container.RegisteredUsers.Add(App.Account.Key);
            }
            catch (Exception ex)
            {
                await DisplayAlert("ERROR", "Error: No container of this id exists.", "OK");
                return;
            }
        }
        await App.ContainerRepo.EditContainer(_container);
        await UpdateTwinProperties();
        await Navigation.PopAsync();
    }

    private async Task UpdateTwinProperties()
    {
        await AzureService.SetDesiredDeviceProperty(_container.DeviceId, ResourceStrings.LowTemperatureThresholdPropertyName, _container.LowTempThreshold);
        await AzureService.SetDesiredDeviceProperty(_container.DeviceId, ResourceStrings.HighTemperatureThresholdPropertyName, _container.HighTempThreshold);

        if (!telemetryIntervalEntry.Text.Equals(null) && !telemetryIntervalEntry.Text.Equals(string.Empty))
            await AzureService.SetDesiredDeviceProperty(_container.DeviceId, ResourceStrings.TelemetryIntervalPropertyName, int.Parse(telemetryIntervalEntry.Text));
    }

    private void lowTempThresholdSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        Slider slider = (Slider)sender;
        double moduloResult = slider.Value % 1;

        // Forces the slider value to only be an integer.
        if (moduloResult != 0)
        {
            slider.Value -= moduloResult;
        }
    }

    private void highTempThresholdSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        Slider slider = (Slider)sender;
        double moduloResult = slider.Value % 1;

        // Forces the slider value to only be an integer.
        if (moduloResult != 0)
        {
            slider.Value -= moduloResult;
        }
    }

    private async void telemetryIntervalEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        Entry entry = (Entry)sender;
        if (entry.Text.Equals(null) || entry.Text.Equals(string.Empty))
            return;

        try
        {
            int telemetryInterval = int.Parse(entry.Text);
        }
        catch(Exception ex)
        {
            await DisplayAlert("Error", "The telemetry interval has to be an integer.", "OK");
            telemetryIntervalEntry.Text = string.Empty;
        }
    }
}
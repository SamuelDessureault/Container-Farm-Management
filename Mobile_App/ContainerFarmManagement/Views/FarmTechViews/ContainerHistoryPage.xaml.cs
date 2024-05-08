using ContainerFarmManagement.Models;
using ContainerFarmManagement.Repos;
using ContainerFarmManagement.Utilities;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Maui.Controls;

namespace ContainerFarmManagement.Views.FarmTechViews;

public partial class ContainerHistoryPage : ContentPage
{
    public List<ISeries> Series { get; set; }
    public Container Container { get; set; }
    public Axis[] XAxis { get; set; }
    public Axis[] YAxis { get; set; }

    public ContainerHistoryPage(Container container)
	{
		InitializeComponent();
        Container = container;
        Title = Container.Name;
        chartPicker.SelectedIndex = 0;
        BindingContext = this;
    }

    private async Task CreateChart(Reading.SensorTypes type, Reading.Units unit)
    {
        try
        {
            // Get requested reading history and create the chart.
            var history = await Container.PlantSubsystem.GetHistory(type, unit, DateTime.MinValue, DateTime.Now);
            Series = ChartsRepo.GetSeries(history.Select(r => r.Value));
            YAxis = ChartsRepo.GetYAxis(unit.Description());
            XAxis = ChartsRepo.GetXAxis();
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "There was an error while performing this task. The chart could not be created.", "OK");
        }
    }

    private async void chartPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            Picker picker = (Picker)sender;
            switch (picker.SelectedItem.ToString())
            {
                case "Temperature":
                    await CreateChart(Reading.SensorTypes.TEMPERATURE, Reading.Units.CELCIUS);
                    break;
                case "Humidity":
                    await CreateChart(Reading.SensorTypes.HUMIDITY, Reading.Units.HUMIDITY);
                    break;
                case "Water Level":
                    await CreateChart(Reading.SensorTypes.WATERLVL, Reading.Units.MILLILITER);
                    break;
                case "Soil Moisture":
                    await CreateChart(Reading.SensorTypes.SOIL_MOISTURE, Reading.Units.MOISTURELVL);
                    break;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "There was an error while performing this task. Cannot change chart at this time.", "OK");
        }
    }

}
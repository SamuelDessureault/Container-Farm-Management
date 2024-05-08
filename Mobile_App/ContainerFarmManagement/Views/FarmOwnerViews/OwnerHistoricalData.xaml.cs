using ContainerFarmManagement.Models;
using ContainerFarmManagement.Repos;
using ContainerFarmManagement.Services;
using ContainerFarmManagement.Utilities;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.Azure.Devices.Shared;
using System.Linq;

namespace ContainerFarmManagement.Views.FarmOwnerViews
{
    public partial class OwnerHistoricalData : ContentPage
    {
        public List<ISeries> Series { get; set; }
        public Axis[] XAxis { get; set; }
        public Axis[] YAxis { get; set; }

        public Container Container { get; set; }

        public OwnerHistoricalData(Container container)
        {
            InitializeComponent();
            Container = container;
            chartPicker.SelectedIndex = 0;
            Title = Container.Name;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = this;
        }

        private async Task CreateChart(Reading.SensorTypes type, Reading.Units unit)
        {
            try
            {
                var history = await Container.SecuritySubsystem.GetHistory(type, unit, DateTime.MinValue, DateTime.Now);
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
                    case "Noise":
                        await CreateChart(Reading.SensorTypes.NOISE, Reading.Units.NOISE);
                        break;
                    case "Luminosity":
                        await CreateChart(Reading.SensorTypes.LUMINOSITY, Reading.Units.UNITLESS);
                        break;
                    case "Motion":
                        await CreateChart(Reading.SensorTypes.MOTION, Reading.Units.UNITLESS);
                        break;
                    case "Door":
                        await CreateChart(Reading.SensorTypes.DOOR, Reading.Units.UNITLESS);
                        break;
                    case "Door Lock":
                        await CreateChart(Reading.SensorTypes.DOOR_LOCK, Reading.Units.UNITLESS);
                        break;

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "There was an error while performing this task. Cannot change chart at this time.", "OK");
            }
        }




    }
}

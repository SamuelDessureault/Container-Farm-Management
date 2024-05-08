using ContainerFarmManagement.Interfaces;
using ContainerFarmManagement.Services;
using ContainerFarmManagement.Utilities;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

///STS technologies
///Semester 6 - 2023-04-27
/// App Dev III
/// Class to handle the plant subsystem, both the quering of readings from it and controlling its actuators

namespace ContainerFarmManagement.Models.SubSystems
{
    public class PlantSubsystem : ISubSystem, INotifyPropertyChanged
    {
        private List<Reading.SensorTypes> sensors;
        private List<Command.ActuatorTypes> actuators;
        private string temperature;
        private string humidity;
        private string waterLevel;
        private string soilMoisture;

        public string Name { get; set; }

        public string Temperature
        {
            get
            {
                return temperature;
            }
            private set
            {
                temperature = value;
                OnPropertyChanged();
            }
        }
        public string Humidity
        {
            get
            {
                return humidity;
            }
            private set
            {
                humidity = value;
                OnPropertyChanged();
            }
        }
        public string WaterLevel
        {
            get
            {
                return waterLevel;
            }
            private set
            {
                waterLevel = value;
                OnPropertyChanged();
            }
        }
        public string SoilMoisture
        {
            get
            {
                return soilMoisture;
            }
            private set
            {
                soilMoisture = value;
                OnPropertyChanged();
            }
        }

        private string deviceId;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<Reading.SensorTypes> Sensors
        {
            get
            {
                return sensors;
            }
        }
        public List<Command.ActuatorTypes> Actuators
        {
            get
            {
                return actuators;
            }
        }
        public PlantSubsystem(string deviceID)
        {
            deviceId = deviceID;
            sensors = new List<Reading.SensorTypes>();
            sensors.Add(Reading.SensorTypes.TEMPERATURE);
            sensors.Add(Reading.SensorTypes.HUMIDITY);
            sensors.Add(Reading.SensorTypes.WATERLVL);
            sensors.Add(Reading.SensorTypes.SOIL_MOISTURE);

            actuators = new List<Command.ActuatorTypes>();
            actuators.Add(Command.ActuatorTypes.FAN);
            actuators.Add(Command.ActuatorTypes.RGB);
            actuators.Add(Command.ActuatorTypes.LOCK);
            App.ReadingRepository.Readings.CollectionChanged += UpdateProperties;


        }

        ~PlantSubsystem()
        {
            App.ReadingRepository.Readings.CollectionChanged -= UpdateProperties;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<int> ControlActuator(Command command)
        {
            if (Actuators.Contains(command.Target))
            {
                try
                {
                    return await AzureService.InvokeMethod(command, deviceId);
                }
                catch (Exception ex)
                {
                    return 404;
                }
            }
            return 403;
        }
        /// <summary>
        /// Get the history of all sensors in the subsystem, within a given time frame
        /// </summary>
        /// <param name="from">The beginning of the time frame</param>
        /// <param name="to">The end of the time frame</param>
        /// <returns>A collention of readings retreived</returns>
        public async Task<ObservableCollection<Reading>> GetAllHistory(DateTime from, DateTime to)
        {
            Array units = System.Enum.GetValues(typeof(Reading.Units));
            List<Reading.Units> unitsList = new List<Reading.Units>();
            foreach (Reading.Units unit in units)
                unitsList.Add(unit);
            return await App.ReadingRepository.GetHistory(deviceId, Sensors, unitsList, from, to);
        }
        /// <summary>
        /// Get the most recent reading from all sensors in the subsystem
        /// </summary>
        /// <returns>A collention of readings retreived</returns>
        public async Task<ObservableCollection<Reading>> GetAllLatest()
        {
            Array units = System.Enum.GetValues(typeof(Reading.Units));
            List<Reading.Units> unitsList = new List<Reading.Units>();
            foreach (Reading.Units unit in units)
                unitsList.Add(unit);
            return await App.ReadingRepository.GetLatest(deviceId, Sensors, unitsList);
        }
        /// <summary>
        /// Get the history of a specific sensor in the subsystem, within a given time frame
        /// </summary>
        /// <param name="sensorType">The type of sensor</param>
        /// <param name="unit">The unit of the reading</param>
        /// <param name="from">The start of the time frame</param>
        /// <param name="to">The end of the time frame</param>
        /// <returns>A Collection of readings retreived.</returns>
        /// <exception cref="ArgumentException">Thrown if the requested sensor type is not part of the subsystem</exception>
        public async Task<ObservableCollection<Reading>> GetHistory(Reading.SensorTypes sensorType, Reading.Units unit, DateTime from, DateTime to)
        {
            if (Sensors.Contains(sensorType))
                return await App.ReadingRepository.GetHistory(deviceId, sensorType, unit, from, to);
            throw new ArgumentException("Requested Sensor is not part of subsystem");
        }
        /// <summary>
        /// Get the history of a collection of sensors in the subsystem, within a given time frame
        /// </summary>
        /// <param name="sensorTypes">The types of sensors</param>
        /// <param name="units">The units of the readings</param>
        /// <param name="from">The start of the time frame</param>
        /// <param name="to">The end of the time frame</param>
        /// <returns>A Collection of readings retreived</returns>
        /// <exception cref="ArgumentException">Thrown if one of the requested sensor types is not part of the subsystem</exception>
        public async Task<ObservableCollection<Reading>> GetHistory(IEnumerable<Reading.SensorTypes> sensorTypes, IEnumerable<Reading.Units> units, DateTime from, DateTime to)
        {
            foreach (Reading.SensorTypes sensorType in sensorTypes)
                if (!Sensors.Contains(sensorType))
                    throw new ArgumentException(string.Format("Sensor type: {0} is not part of subsystem", sensorType.Description()));
            return await App.ReadingRepository.GetHistory(deviceId, sensorTypes, units, from, to);
        }
        /// <summary>
        /// Gets the latest reading from a given sensor
        /// </summary>
        /// <param name="sensorType">The sensor type</param>
        /// <param name="unit">The unit of the reading</param>
        /// <returns>A instance of class reading</returns>
        /// <exception cref="ArgumentException">Thrown if the requested sensor type is not part of the subsystem</exception>
        public async Task<Reading> GetLatest(Reading.SensorTypes sensorType, Reading.Units unit)
        {
            if (Sensors.Contains(sensorType))
                return await App.ReadingRepository.GetLatest(deviceId, sensorType, unit);
            throw new ArgumentException("Requested Sensor is not part of subsystem");
        }
        /// <summary>
        /// Gets the latest reading of a collection of sensors.
        /// </summary>
        /// <param name="sensorTypes">The types of sensors</param>
        /// <param name="units">The units of the readings</param>
        /// <returns>A Collection of readings retreived</returns>
        /// <exception cref="ArgumentException">Thrown if one of the requested sensor types is not part of the subsystem</exception>
        public async Task<ObservableCollection<Reading>> GetLatest(IEnumerable<Reading.SensorTypes> sensorTypes, IEnumerable<Reading.Units> units)
        {
            foreach (Reading.SensorTypes sensorType in sensorTypes)
                if (!Sensors.Contains(sensorType))
                    throw new ArgumentException(string.Format("Sensor type: {0} is not part of subsystem", sensorType.Description()));
            return await App.ReadingRepository.GetLatest(deviceId, sensorTypes, units);
        }

        public async void UpdateProperties(object? sender, NotifyCollectionChangedEventArgs e)
        {
            await UpdateData();
        }

        public async Task UpdateData()
        {
            try
            {
                Reading temperatureReading = await GetLatest(Reading.SensorTypes.TEMPERATURE, Reading.Units.CELCIUS);
                Temperature = $"{float.Round(temperatureReading.Value, 2)} {temperatureReading.Unit.Description()}";
            }
            catch (Exception ex)
            {
                Temperature = "No Data";
            }

            try
            {
                Reading humidityReading = await GetLatest(Reading.SensorTypes.HUMIDITY, Reading.Units.HUMIDITY);
                Humidity = $"{float.Round(humidityReading.Value, 2)} {humidityReading.Unit.Description()}";
            }
            catch (Exception ex)
            {
                Humidity = "No Data";
            }

            try
            {
                Reading waterLevelReading = await GetLatest(Reading.SensorTypes.WATERLVL, Reading.Units.MILLILITER);
                WaterLevel = $"{float.Round(waterLevelReading.Value, 2)} {waterLevelReading.Unit.Description()}";
            }
            catch (Exception ex)
            {
                WaterLevel = "No Data";
            }

            try
            {
                Reading soilMoistureReading = await GetLatest(Reading.SensorTypes.SOIL_MOISTURE, Reading.Units.MOISTURELVL);
                SoilMoisture = $"{float.Round(soilMoistureReading.Value, 2)} {soilMoistureReading.Unit.Description()}";
            }
            catch (Exception ex)
            {
                SoilMoisture = "No Data";
            }
        }
    }
}
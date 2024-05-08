using ContainerFarmManagement.Interfaces;
using ContainerFarmManagement.Services;
using ContainerFarmManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

///STS technologies
///Semester 6 - 2023-04-27
/// App Dev III
/// Class to handle the security subsystem, both the quering of readings from it and controlling its actuators

namespace ContainerFarmManagement.Models.SubSystems
{
    public class SecuritySubsystem : ISubSystem, INotifyPropertyChanged
    {
        private List<Reading.SensorTypes> sensors;
        private List<Command.ActuatorTypes> actuators;
        private string noise;
        private string luminosity;
        private string motion;
        private string door;

        public string Name { get; set; }

        public string Noise
        {
            get
            {
                return noise;
            }
            private set
            {
                noise = value;
                OnPropertyChanged();
            }
        }
        public string Luminosity
        {
            get
            {
                return luminosity;
            }
            private set
            {
                luminosity = value;
                OnPropertyChanged();
            }
        }
        public string Motion
        {
            get
            {
                return motion;
            }
            private set
            {
                motion = value;
                OnPropertyChanged();
            }
        }
        public string Door
        {
            get
            {
                return door;
            }
            private set
            {
                door = value;
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
        public SecuritySubsystem(string deviceID)
        {
            deviceId = deviceID;
            sensors = new List<Reading.SensorTypes>();
            sensors.Add(Reading.SensorTypes.NOISE);
            sensors.Add(Reading.SensorTypes.LUMINOSITY);
            sensors.Add(Reading.SensorTypes.MOTION);
            sensors.Add(Reading.SensorTypes.DOOR);

            actuators = new List<Command.ActuatorTypes>();
            actuators.Add(Command.ActuatorTypes.LOCK);
            actuators.Add(Command.ActuatorTypes.BUZZER);

            App.ReadingRepository.Readings.CollectionChanged += UpdateProperties;
        }

        ~SecuritySubsystem()
        {
            App.ReadingRepository.Readings.CollectionChanged -= UpdateProperties;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Method to control an actuator in the subsystem
        /// </summary>
        /// <param name="command">The command to be sent</param>
        /// <returns>Boolean, based on if the command was successfully executed.</returns>
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

        public async void UpdateProperties(object sender, NotifyCollectionChangedEventArgs e)
        {
            await UpdateData();
        }
        public async Task UpdateData()
        {
            try
            {
                Reading noiseReading = await GetLatest(Reading.SensorTypes.NOISE, Reading.Units.NOISE);
                Noise = $"{noiseReading.Value} {noiseReading.Unit.Description()}";
            }
            catch (Exception ex)
            {
                Noise = "No Data";
            }
            try
            {
                Reading luminosityReading = await GetLatest(Reading.SensorTypes.LUMINOSITY, Reading.Units.UNITLESS);
                Luminosity = $"{luminosityReading.Value} {luminosityReading.Unit.Description()}";
            }
            catch (Exception ex)
            {
                Luminosity = "No Data";
            }
            try
            {
                Reading motionReading = await GetLatest(Reading.SensorTypes.MOTION, Reading.Units.UNITLESS);
                Motion = $"{motionReading.Value} {motionReading.Unit.Description()}";
            }
            catch (Exception ex)
            {
                Motion = "No Data";
            }
            try
            {
                Reading doorReading = await GetLatest(Reading.SensorTypes.DOOR, Reading.Units.UNITLESS);
                Door = $"{doorReading.Value} {doorReading.Unit.Description()}";
            }
            catch (Exception ex)
            {
                Door = "No Data";
            }
        }
    }
}
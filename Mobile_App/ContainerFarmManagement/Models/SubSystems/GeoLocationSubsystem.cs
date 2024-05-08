using ContainerFarmManagement.Interfaces;
using ContainerFarmManagement.Services;
using ContainerFarmManagement.Utilities;
using Maui.GoogleMaps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

///STS technologies
///Semester 6 - 2023-04-27
/// App Dev III
/// Class to handle the Goelocation subsystem, both the quering of readings from it and controlling its actuators

namespace ContainerFarmManagement.Models.SubSystems
{
    public class GeoLocationSubsystem : ISubSystem, INotifyPropertyChanged
    {
        private List<Reading.SensorTypes> sensors;
        private List<Command.ActuatorTypes> actuators;
        private float latitude;
        private float longitude;
        private string pitch;
        private string roll;
        private string vibration;

        public string Name { get; set; }

        public string Pitch
        {
            get
            {
                return pitch;
            }
            private set
            {
                pitch = value;
                OnPropertyChanged();
            }
        }
        public string Roll
        {
            get
            {
                return roll;
            }
            private set
            {
                roll = value;
                OnPropertyChanged();
            }
        }
        public string Vibration
        {
            get
            {
                return vibration;
            }
            private set
            {
                vibration = value;
                OnPropertyChanged();
            }
        }
        public float Latitude
        {
            get
            {
                return latitude;
            }
            private set
            {
                latitude = value;
                OnPropertyChanged();
            }
        }
        public float Longitude
        {
            get
            {
                return longitude;
            }
            private set
            {
                longitude = value;
                OnPropertyChanged();
            }
        }


        public string FormattedPosition
        {
            get
            {
                try
                {
                    return $"Latitude: {Latitude}, Longitude: {Longitude}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting formated geoloc : {ex.Message}");
                }
                return "";
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
        public GeoLocationSubsystem(string deviceID)
        {
            deviceId = deviceID;
            sensors = new List<Reading.SensorTypes>();
            sensors.Add(Reading.SensorTypes.GPS);
            sensors.Add(Reading.SensorTypes.PITCH_ROLL);
            sensors.Add(Reading.SensorTypes.VIBRATION);
            actuators = new List<Command.ActuatorTypes>();

            App.ReadingRepository.Readings.CollectionChanged += UpdateProperties;
        }

        ~GeoLocationSubsystem()
        {
            App.ReadingRepository.Readings.CollectionChanged -= UpdateProperties;
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Controls an actuator by its command.
        /// </summary>
        /// <param name="command">The command for the actuator.</param>
        /// <returns>True is the command was successful, false otherwise.</returns>
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
                Reading pitchReading = await GetLatest(Reading.SensorTypes.PITCH_ROLL, Reading.Units.PITCH);
                Pitch = $"{pitchReading?.Value}";
            }
            catch (Exception ex)
            {
                Pitch = "No Data";
            }
            try
            {
                Reading rollReading = await GetLatest(Reading.SensorTypes.PITCH_ROLL, Reading.Units.ROLL);
                Roll = $"{rollReading?.Value}";
            }
            catch (Exception ex)
            {
                Roll = "No Data";
            }
            try
            {
                Reading vibrationReading = await GetLatest(Reading.SensorTypes.VIBRATION, Reading.Units.VIBRATION);

                Vibration = $"{vibrationReading?.Value}";
            }
            catch (Exception ex)
            {
                Vibration = "No Data";
            }
            try
            {
                Reading latitudeReading = await GetLatest(Reading.SensorTypes.GPS, Reading.Units.DEGREES_LATITUDE);
                Reading longitudeReading = await GetLatest(Reading.SensorTypes.GPS, Reading.Units.DEGREES_LONGITUDE);

                if (latitudeReading != null && longitudeReading != null)
                {
                    Latitude = latitudeReading.Value;
                    Longitude = longitudeReading.Value;
                }
                else
                {
                    Latitude = 0;
                    Longitude = 0;
                }
            }
            catch (Exception ex)
            {
                Latitude = 0;
                Longitude = 0;
            }
        }
    }
}
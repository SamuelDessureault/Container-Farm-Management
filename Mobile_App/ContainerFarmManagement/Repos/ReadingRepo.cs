using ContainerFarmManagement.Models;
using ContainerFarmManagement.Services;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ContainerFarmManagement.Repos
{
    public class ReadingRepo
    {
        /// <summary>
        /// The list of Reading items in the database
        /// </summary>
        public ObservableCollection<Reading> Readings { get; private set; }
        public ReadingRepo()
        {
            Readings ??= new ObservableCollection<Reading>();
        }

        /// <summary>
        /// Gets the latest reading from a given sensor
        /// </summary>
        /// <param name="sensorType">The sensor type</param>
        /// <param name="unit">The unit of the reading</param>
        /// <returns>A instance of class reading</returns>
        public async Task<Reading> GetLatest(string deviceId, Reading.SensorTypes sensorType, Reading.Units unit)
        {
            List<Reading> result = Readings.Where(r => r.DeviceId == deviceId && r.SensorType == sensorType && r.Unit == unit)
                .OrderByDescending(r => r.TimeStamp).Take(1).ToList();
            if (result.Count == 0)
                return null;
            return result[0];
        }

        /// <summary>
        /// Gets the latest reading of a collection of sensors.
        /// </summary>
        /// <param name="sensorTypes">The types of sensors</param>
        /// <param name="units">The units of the readings</param>
        /// <returns>A Collection of readings retreived</returns>
        public async Task<ObservableCollection<Reading>> GetLatest(string deviceId, IEnumerable<Reading.SensorTypes> sensorTypes, IEnumerable<Reading.Units> units)
        {
            ObservableCollection<Reading> results = new ObservableCollection<Reading>();
            foreach (Reading.SensorTypes sensorType in sensorTypes)
            {
                foreach (Reading.Units unit in units)
                {
                    Reading result = await GetLatest(deviceId, sensorType, unit);
                    if (result != null)
                        results.Add(result);
                }
            }
            return results;
        }

        /// <summary>
        /// Get the history of all sensors in the database, within a container.
        /// </summary>
        /// <param name="deviceId">the container's id.</param>
        /// <returns>A collention of readings retreived</returns>
        public async Task<ObservableCollection<Reading>> GetAllLatest(string deviceId)
        {
            Array sensorTypes = Enum.GetValues(typeof(Reading.SensorTypes));
            Array units = Enum.GetValues(typeof(Reading.Units));

            List<Reading.SensorTypes> sensorTypesList = new List<Reading.SensorTypes>();
            foreach (Reading.SensorTypes sensorType in sensorTypes)
                sensorTypesList.Add(sensorType);

            List<Reading.Units> unitsList = new List<Reading.Units>();
            foreach (Reading.Units unit in units)
                unitsList.Add(unit);

            return await GetLatest(deviceId, sensorTypesList, unitsList);
        }

        /// <summary>
        /// Get the history of a specific sensor in the database, within a given time frame
        /// </summary>
        /// <param name="deviceId">The container's key.</param>
        /// <param name="sensorType">The type of sensor</param>
        /// <param name="unit">The unit of the reading</param>
        /// <param name="from">The start of the time frame</param>
        /// <param name="to">The end of the time frame</param>
        /// <returns>A Collection of readings retreived.</returns>
        public async Task<ObservableCollection<Reading>> GetHistory(string deviceId, Reading.SensorTypes sensorType, Reading.Units unit, DateTime from, DateTime to)
        {
            List<Reading> results = Readings.Where(r => r.ContainerKey == deviceId && r.SensorType == sensorType && r.Unit == unit)
                .Where(r => r.TimeStamp >= from && r.TimeStamp <= to).ToList();

            return new ObservableCollection<Reading>(results);
        }

        /// <summary>
        /// Get the history of a collection of sensors in the database, within a given time frame
        /// </summary>
        /// <param name="deviceId">The container's key.</param>
        /// <param name="sensorTypes">The types of sensors</param>
        /// <param name="units">The units of the readings</param>
        /// <param name="from">The start of the time frame</param>
        /// <param name="to">The end of the time frame</param>
        /// <returns>A Collection of readings retreived</returns>
        public async Task<ObservableCollection<Reading>> GetHistory(string deviceId, IEnumerable<Reading.SensorTypes> sensorTypes, IEnumerable<Reading.Units> units, DateTime from, DateTime to)
        {
            ObservableCollection<Reading> results = new ObservableCollection<Reading>();
            foreach (Reading.SensorTypes sensorType in sensorTypes)
            {
                foreach (Reading.Units unit in units)
                {
                    ObservableCollection<Reading> result = await GetHistory(deviceId, sensorType, unit, from, to);
                    if (result != null && result.Count > 0)
                    {
                        foreach (Reading reading in result)
                            results.Add(reading);
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Get the history of all sensors in the database, within a given time frame
        /// </summary>
        /// <param name="from">The beginning of the time frame</param>
        /// <param name="to">The end of the time frame</param>
        /// <returns>A collention of readings retreived</returns>
        public async Task<ObservableCollection<Reading>> GetAllHistory(string deviceId, DateTime from, DateTime to)
        {
            Array sensorTypes = Enum.GetValues(typeof(Reading.SensorTypes));
            Array units = Enum.GetValues(typeof(Reading.Units));

            List<Reading.SensorTypes> sensorTypesList = new List<Reading.SensorTypes>();
            foreach (Reading.SensorTypes sensorType in sensorTypes)
                sensorTypesList.Add(sensorType);

            List<Reading.Units> unitsList = new List<Reading.Units>();
            foreach (Reading.Units unit in units)
                unitsList.Add(unit);

            return await GetHistory(deviceId, sensorTypesList, unitsList, from, to);
        }

        /// <summary>
        /// Add a reading to the database.
        /// </summary>
        /// <param name="reading">The reading object to be added.</param>
        /// <returns>True if the reading was added to the database, false otherwise.</returns>
        public async Task<bool> AddReading(Reading reading)
        {
            try
            {
                Readings.Add(reading);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Removes a reading object from the database.
        /// </summary>
        /// <param name="reading">The reading object to be removed.</param>
        /// <returns>True is the reading was removed, false otherwise.</returns>
        public async Task<bool> RemoveReading(Reading reading)
        {
            try
            {
                Readings.Remove(reading);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
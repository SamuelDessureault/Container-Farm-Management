using ContainerFarmManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Interfaces
{
    internal interface ISubSystem
    {
        /// <summary>
        /// The subsystem's name.
        /// </summary>
        public string Name { get; set; }
        public List<Reading.SensorTypes> Sensors { get; }
        public Task<Reading> GetLatest(Reading.SensorTypes sensorType, Reading.Units unit);
        public Task<ObservableCollection<Reading>> GetLatest(IEnumerable<Reading.SensorTypes> sensorTypes, IEnumerable<Reading.Units> units);
        public Task<ObservableCollection<Reading>> GetAllLatest();
        public Task<ObservableCollection<Reading>> GetHistory(Reading.SensorTypes sensorType, Reading.Units unit, DateTime from, DateTime to);
        public Task<ObservableCollection<Reading>> GetHistory(IEnumerable<Reading.SensorTypes> sensorTypes, IEnumerable<Reading.Units> units, DateTime from, DateTime to);
        public Task<ObservableCollection<Reading>> GetAllHistory(DateTime from, DateTime to);
        public Task<int> ControlActuator(Models.Command command);
        public void UpdateProperties(object? sender, NotifyCollectionChangedEventArgs e);
    }
}
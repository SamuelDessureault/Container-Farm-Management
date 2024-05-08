using ContainerFarmManagement.Repos;
using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContainerFarmManagement.Models;
using ContainerFarmManagement.Models.SubSystems;
using System.ComponentModel;
using ContainerFarmManagement.Interfaces;

namespace ContainerFarmManagement.Models
{
    public class Container : INotifyPropertyChanged, IHasKey
    {
        public const float DEFAULT_LOW_TEMP_THRESHOLD = 5;
        public const float DEFAULT_HIGH_TEMP_THRESHOLD = 30;

        private GeoLocationSubsystem _geoLocationSubsystem;
        private PlantSubsystem _plantSubsystem;
        private SecuritySubsystem _securitySubsystem;
        private const string CONTROL_ACTUATORS = "Value TBD";
        private ServiceClient _serviceClient;
        private List<string> _registeredUsers;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The container's key in the database.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The container's name.
        /// </summary>
        public string Name { get; set; }

        public string DeviceId { get; set; }
        public float LowTempThreshold { get; set; } = DEFAULT_LOW_TEMP_THRESHOLD;
        public float HighTempThreshold { get; set; } = DEFAULT_HIGH_TEMP_THRESHOLD;
        public string CropName { get; set; }


        /// <summary>
        /// The list of user keys registered to this container.
        /// </summary>
        public List<string> RegisteredUsers { get { return _registeredUsers; } }

        /// <summary>
        /// The container's geo location subsystem.
        /// </summary>
        public GeoLocationSubsystem GeoLocationSubsystem { get { return _geoLocationSubsystem ??= new GeoLocationSubsystem(DeviceId); } }

        /// <summary>
        /// The container's plant subsystem.
        /// </summary>
        public PlantSubsystem PlantSubsystem { get { return _plantSubsystem ??= new PlantSubsystem(DeviceId); } }

        /// <summary>
        /// The container's security subsystem.
        /// </summary>
        public SecuritySubsystem SecuritySubsystem { get { return _securitySubsystem ??= new SecuritySubsystem(DeviceId); } }

        public Container()
        {
            Key = "";
            Name = "";
            _serviceClient = null;
            DeviceId = "";
            _plantSubsystem = null;
            _geoLocationSubsystem = null;
            _securitySubsystem = null;
            _registeredUsers = new List<string>();
        }

        public Container(string deviceId)
        {
            Key = "";
            Name = "";
            _serviceClient = null;
            DeviceId = deviceId;
            _plantSubsystem = null;
            _geoLocationSubsystem = null;
            _securitySubsystem = null;
            _registeredUsers = new List<string>();
        }

        public Container(Container container)
        {
            Key = container.Key;
            Name = container.Name;
            _serviceClient = container._serviceClient;
            DeviceId = container.DeviceId;
            _plantSubsystem = container._plantSubsystem;
            _geoLocationSubsystem = container._geoLocationSubsystem;
            _securitySubsystem = container._securitySubsystem;
            _registeredUsers = container._registeredUsers;
        }

        public Container(string name, ServiceClient service, string deviceId)
        {
            Key = "";
            Name = name;
            _serviceClient = service;
            DeviceId = deviceId;
            _plantSubsystem = new PlantSubsystem(DeviceId);
            CropName = "No Crops Yet";
            _geoLocationSubsystem = new GeoLocationSubsystem(DeviceId);
            _securitySubsystem = new SecuritySubsystem(DeviceId);
            _registeredUsers = new List<string>();
        }

        public Container(string name, ServiceClient service, string deviceId, string cropName)
        {
            Key = "";
            Name = name;
            _serviceClient = service;
            DeviceId = deviceId;
            _plantSubsystem = new PlantSubsystem(DeviceId);
            CropName = cropName;
            _geoLocationSubsystem = new GeoLocationSubsystem(DeviceId);
            _securitySubsystem = new SecuritySubsystem(DeviceId);
            _registeredUsers = new List<string>();
        }

        // Invoke the direct method on the device, passing the payload.
        private static async Task<bool> InvokeMethodAsync(string deviceId, ServiceClient serviceClient, string methodName, string payload)
        {
            try
            {
                var methodInvocation = new CloudToDeviceMethod(methodName)
                {
                    ResponseTimeout = TimeSpan.FromSeconds(30),
                };
                methodInvocation.SetPayloadJson(payload);

                // Invoke the direct method asynchronously and get the response from the simulated device.
                CloudToDeviceMethodResult response = await serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

                return response.Status == 200;
            }
            catch (Exception e)
            {
                return false;
            }

        }
    }
}
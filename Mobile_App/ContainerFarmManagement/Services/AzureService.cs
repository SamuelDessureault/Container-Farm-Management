using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using ContainerFarmManagement.Models;
using ContainerFarmManagement.Utilities;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Maui.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Services
{
    public class AzureService
    {
        private static RegistryManager registry;
        private static RegistryManager Registry_Manager
        {
            get
            {
                return registry ??= RegistryManager.CreateFromConnectionString(App.Settings.HubConnectionString);
            }
            set
            {
                registry = value;
            }
        }

        private static Dictionary<string, Reading.Units> unitMapping = new Dictionary<string, Reading.Units>()
        {
            { "%", Reading.Units.MOISTURELVL },
            { "mL", Reading.Units.MILLILITER },
            { "noise", Reading.Units.NOISE },
            { "vibration", Reading.Units.VIBRATION },
            { "roll", Reading.Units.ROLL },
            { "pitch", Reading.Units.PITCH },
            { "degrees-longitude", Reading.Units.DEGREES_LONGITUDE },
            { "degrees-latitude", Reading.Units.DEGREES_LATITUDE },
            { "% HR", Reading.Units.HUMIDITY },
            { "C", Reading.Units.CELCIUS },
            { "", Reading.Units.UNITLESS }
        };

        private static Dictionary<string, Reading.SensorTypes> sensorTypesMapping = new Dictionary<string, Reading.SensorTypes>()
        {
            { "temperature", Reading.SensorTypes.TEMPERATURE },
            { "humidity", Reading.SensorTypes.HUMIDITY },
            { "gps", Reading.SensorTypes.GPS },
            { "pitch-roll", Reading.SensorTypes.PITCH_ROLL },
            { "vibration", Reading.SensorTypes.VIBRATION },
            { "noise", Reading.SensorTypes.NOISE },
            { "motion", Reading.SensorTypes.MOTION },
            { "water", Reading.SensorTypes.WATERLVL },
            { "luminosity", Reading.SensorTypes.LUMINOSITY },
            { "soil-moisture", Reading.SensorTypes.SOIL_MOISTURE },
            { "door", Reading.SensorTypes.DOOR}
        };

        public static async Task<bool> RegisterDevice(string id)
        {
            try
            {
                var result = await Registry_Manager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(id));
                if (result != null)
                    return true;
                return false;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;//return an empty array
            }
        }
        public static async Task<TwinProperties> GetDeviceProperties(string deviceId)
        {
            try
            {
                var twin = await Registry_Manager.GetTwinAsync(deviceId);
                return twin.Properties;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
        public static async Task<bool> SetDesiredDeviceProperty(string deviceId, string key, object value)
        {
            try
            {
                var twin = await Registry_Manager.GetTwinAsync(deviceId);
                twin.Properties.Desired[key] = value;
                await Registry_Manager.UpdateTwinAsync(twin.DeviceId, twin, twin.ETag);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        private static ServiceClient serviceClient;

        private const string CONTROL_ACTUATORS_METHOD_NAME = "controlactuators";//might need to change based on the method name on the device
        private const string TARGET_DICT_KEY = "target";
        public static async Task<int> InvokeMethod(ContainerFarmManagement.Models.Command command, string deviceId)
        {
            try
            {
                Dictionary<string, object> payloadDict = new Dictionary<string, object>();
                foreach (var key in command.Values.Keys)
                {
                    payloadDict[key] = command.Values[key];
                }
                payloadDict[TARGET_DICT_KEY] = command.Target.Description();
                string payloadJSON = JsonConvert.SerializeObject(payloadDict);

                return await AzureService.InvokeMethodAsync(deviceId, serviceClient, CONTROL_ACTUATORS_METHOD_NAME, payloadJSON);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return -1;
            }
        }

        private static async Task<int> InvokeMethodAsync(string deviceId, ServiceClient serviceClient, string methodName, string payload)
        {
            try
            {
                var methodInvocation = new CloudToDeviceMethod(methodName)
                {
                    ResponseTimeout = TimeSpan.FromSeconds(30),
                };
                methodInvocation.SetPayloadJson(payload);

                Debug.WriteLine($"Invoking direct method for device: {deviceId}");

                // Invoke the direct method asynchronously and get the response from the simulated device.
                CloudToDeviceMethodResult response = await serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);

                Debug.WriteLine($"Response status: {response.Status}, payload:\n\t{response.GetPayloadAsJson()}");
                return response.Status;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return -1;
            }

        }
        public AzureService()
        {
            storageConnectionString = App.Settings.StorageConnectionString;
            blobContainerName = App.Settings.BlobContainerName;

            eventHubsConnectionString = App.Settings.EventHubConnectionString;
            eventHubName = App.Settings.EventHubName;
            consumerGroup = App.Settings.ConsumerGroup;

            storageClient = new BlobContainerClient(
                storageConnectionString,
                blobContainerName);

            processor = new EventProcessorClient(
                storageClient,
                consumerGroup,
                eventHubsConnectionString,
                eventHubName);

            cancellationSource = new CancellationTokenSource();

            serviceClient = ServiceClient.CreateFromConnectionString(App.Settings.HubConnectionString);
        }

        private ConcurrentDictionary<string, int> partitionEventCount = new ConcurrentDictionary<string, int>();

        private bool DoesReadingExist(Reading reading)
        {
            var matchedReadings = App.ReadingRepository.Readings.Where(r => r.DeviceId == reading.DeviceId && r.TimeStamp.Equals(reading.TimeStamp)).ToList();
            return matchedReadings.Any();
        }

        private async Task processEventHandler(ProcessEventArgs args)
        {
            try
            {
                // If the cancellation token is signaled, then the
                // processor has been asked to stop.  It will invoke
                // this handler with any events that were in flight;
                // these will not be lost if not processed.
                //
                // It is up to the handler to decide whether to take
                // action to process the event or to cancel immediately.

                if (args.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                string partition = args.Partition.PartitionId;
                byte[] eventBody = args.Data.EventBody.ToArray();
                //Debug.WriteLine($"Event from partition {partition} with length {eventBody.Length}.");

                try
                {
                    string utfString = Encoding.UTF8.GetString(eventBody, 0, eventBody.Length);
                    Debug.WriteLine(utfString);

                    Dictionary<string, string> receivedValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(utfString);

                    float value = float.Parse(receivedValues["value"]);
                    string deviceId = receivedValues["deviceId"];
                    DateTime timestamp = DateTime.Parse(receivedValues["timestamp"]);

                    string unitAsString = receivedValues["unit"];
                    string typeAsString = receivedValues["type"];

                    Reading.Units unit = Reading.Units.VIBRATION;//set to a random default
                    Reading.SensorTypes type = Reading.SensorTypes.TEMPERATURE;//set to a random default

                    AzureService.unitMapping.TryGetValue(unitAsString, out unit);

                    //switch (unitAsString)
                    //{
                    //    case "C":
                    //        unit = Reading.Units.CELCIUS;
                    //        break;
                    //    case "% HR":
                    //        unit = Reading.Units.HUMIDITY;
                    //        break;
                    //    case "degrees-latitude":
                    //        unit = Reading.Units.DEGREES_LATITUDE;
                    //        break;
                    //    case "degrees-longitude":
                    //        unit = Reading.Units.DEGREES_LONGITUDE;
                    //        break;
                    //    case "pitch":
                    //        unit = Reading.Units.PITCH;
                    //        break;
                    //    case "roll":
                    //        unit = Reading.Units.ROLL;
                    //        break;
                    //    case "vibration":
                    //        unit = Reading.Units.VIBRATION;
                    //        break;
                    //    case "noise":
                    //        unit = Reading.Units.NOISE;
                    //        break;
                    //    case "mL":
                    //        unit = Reading.Units.MILLILITER;
                    //        break;
                    //    case "%":
                    //        unit = Reading.Units.MOISTURELVL;
                    //        break;
                    //}

                    AzureService.sensorTypesMapping.TryGetValue(typeAsString, out type);
                    //switch (typeAsString)
                    //{
                    //    case "temperature":
                    //        type = Reading.SensorTypes.TEMPERATURE;
                    //        break;
                    //    case "humidity":
                    //        type = Reading.SensorTypes.HUMIDITY;
                    //        break;
                    //    case "gps":
                    //        type = Reading.SensorTypes.GPS;
                    //        break;
                    //    case "pitch-roll":
                    //        type = Reading.SensorTypes.PITCH_ROLL;
                    //        break;
                    //    case "vibration":
                    //        type = Reading.SensorTypes.VIBRATION;
                    //        break;
                    //    case "noise":
                    //        type = Reading.SensorTypes.NOISE;
                    //        break;
                    //    case "motion":
                    //        type = Reading.SensorTypes.MOTION;
                    //        break;
                    //    case "water":
                    //        type = Reading.SensorTypes.WATERLVL;
                    //        break;
                    //    case "luminosity":
                    //        type = Reading.SensorTypes.LUMINOSITY;
                    //        break;
                    //    case "soil-moisture":
                    //        type = Reading.SensorTypes.SOIL_MOISTURE;
                    //        break;
                    //}

                    //string containerId = App.ContainerRepo.GetContainerByDeviceId(deviceId)?.Key;
                    //if (containerId == null)//no matching container found
                    //    containerId = string.Empty;
                    Reading reading = new Reading(deviceId, type, unit, value, timestamp)
                    {
                        DeviceId = deviceId
                    };
                    bool isSuccess = await App.ReadingRepository.AddReading(reading);
                    Debug.WriteLine($"Reading was added succesfully: {isSuccess}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR! with parsing data within Azure Service");
                }

                int eventsSinceLastCheckpoint = partitionEventCount.AddOrUpdate(
                    key: partition,
                    addValue: 1,
                    updateValueFactory: (_, currentCount) => currentCount + 1);

                if (eventsSinceLastCheckpoint >= 50)
                {
                    await args.UpdateCheckpointAsync();
                    partitionEventCount[partition] = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private CancellationTokenSource cancellationSource;
        private string storageConnectionString;
        private string blobContainerName;
        private string eventHubsConnectionString;
        private string eventHubName;
        private string consumerGroup;

        private BlobContainerClient storageClient;
        private EventProcessorClient processor;

        public bool IsProcessing
        {
            get { return processor.IsRunning; }
        }
        private Task processErrorHandler(ProcessErrorEventArgs args)
        {
            try
            {
                Debug.WriteLine("Error in the EventProcessorClient");
                Debug.WriteLine($"\tOperation: {args.Operation}");
                Debug.WriteLine($"\tException: {args.Exception}");
                Debug.WriteLine("");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return Task.CompletedTask;
        }

        public async void StartReading()
        {
            processor.ProcessEventAsync += processEventHandler;
            processor.ProcessErrorAsync += processErrorHandler;

            await processor.StartProcessingAsync();

            try
            {
                // The processor performs its work in the background; block until cancellation
                // to allow processing to take place.

                await Task.Delay(Timeout.Infinite, cancellationSource.Token);
            }
            catch (TaskCanceledException)
            {
                // This is expected when the delay is canceled.
            }

            try
            {
                await processor.StopProcessingAsync();
            }
            finally
            {
                // To prevent leaks, the handlers should be removed when processing is complete.

                processor.ProcessEventAsync -= processEventHandler;
                processor.ProcessErrorAsync -= processErrorHandler;
            }
        }
        public void CancelReading()
        {
            cancellationSource.Cancel();
            //try
            //{
            //    processor.StopProcessingAsync();
            //    processor.ProcessEventAsync -= processEventHandler;
            //    processor.ProcessErrorAsync -= processErrorHandler;
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.Message);
            //}
        }
    }
}
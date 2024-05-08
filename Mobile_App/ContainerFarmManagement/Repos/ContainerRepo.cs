using System;
using System.Collections.Generic;
using ContainerFarmManagement.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using static System.Net.Mime.MediaTypeNames;
using ContainerFarmManagement.Services;
using Microsoft.Azure.Devices;

namespace ContainerFarmManagement.Repos
{
    public class ContainerRepo
    {
        private readonly DatabaseService<Container> container_db = App.DataRepo.ContainerDB;

        public ObservableCollection<Container> Containers
        {
            get
            {
                return container_db.Items;
            }
        }
        public ContainerRepo()
        {
            if (Containers.Count == 0)
                AddTestContainer().ConfigureAwait(false);
        }

        private async Task AddTestContainer()
        {
            ServiceClient client = ServiceClient.CreateFromConnectionString(App.Settings.HubConnectionString);

            //THE LINE BELOW DOES NOT WORK. MUST BE WORKED ON.
            //await AzureService.RegisterDevice(App.Settings.DeviceId);

            Container container = new Container("Container Farm 1", client, App.Settings.DeviceId, "Veggies");
            container.RegisteredUsers.Add(App.Account.Key);
            await AddContainer(container);
        }

        /// <summary>
        /// Adds a container to the database.
        /// </summary>
        /// <param name="container">The container object to be added.</param>
        public async Task AddContainer(Container container)
        {
            await container_db.AddItemAsync(container);
        }

        /// <summary>
        /// Removes a container from the database.
        /// </summary>
        /// <param name="container"></param>
        public async Task RemoveContainer(Container container)
        {
            await container_db.DeleteItemAsync(container);
        }

        /// <summary>
        /// Updates a container int the database.
        /// </summary>
        /// <param name="container"></param>
        public async Task EditContainer(Container container)
        {
            await container_db.UpdateItemAsync(container);
        }

        public async Task RemoveAllContainers()
        {
            foreach (Container container in Containers)
            {
                await RemoveContainer(container);
            }
        }

        /// <summary>
        /// Gets a container in the database from its key.
        /// </summary>
        /// <param name="key">The key of the container.</param>
        /// <returns>The container object.</returns>
        public async Task<Container> GetContainer(string key)
        {
            return Containers.ToList().Find(c => c.Key == key);
        }

        /// <summary>
        /// Gets the list of containers from their registered user's key.
        /// </summary>
        /// <param name="userKkey">The user's key.</param>
        /// <returns>The list of containers.</returns>
        public async Task<IEnumerable<Container>> GetContainers(string userKey)
        {
            return container_db.Items.Select(c => c).Where(c => c.RegisteredUsers.Contains(userKey)).ToList();
        }

        /// <summary>
        /// Gets a container in the repo by its name.
        /// </summary>
        /// <param name="name">The name of the container.</param>
        /// <returns>The container object.</returns>
        public Container GetContainerByName(string name)
        {
            return Containers.ToList().Find(c => c.Name == name);
        }

        public Container GetContainerByDeviceId(string deviceId)
        {
            return Containers.ToList().Find(c => String.Compare(c.DeviceId, deviceId) == 0);
        }

        public bool IsContainerRegistered(Container container)
        {
            foreach (Container cont in Containers)
            {
                if (String.Compare(cont.DeviceId, container.DeviceId) == 0)
                    return true;
            }
            return false;
        }
        //public async Task<List<string>> GetUnregisteredDevices()
        //{
        //    List<string> deviceIds = await AzureService.GetAllDevices(); //get all devices from IOT HUB
        //    List<string> registeredDeviceIds = Containers.Select(d => d.DeviceId).ToList(); //all the device ids saved in db
        //    List<string> unregisteredDeviceIds = new List<string>();//where we put the devices connected to the hub but not saved to database

        //    foreach (string deviceId in deviceIds)
        //    {
        //        if (registeredDeviceIds.Contains(deviceId))
        //            unregisteredDeviceIds.Add(deviceId);
        //    }
        //    return unregisteredDeviceIds;
        //}
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Config
{
    public class Settings
    {
        public string EventHubConnectionString { get; set; }
        public string EventHubName { get; set; }
        public string ConsumerGroup { get; set; }
        public string StorageConnectionString { get; set; }
        public string BlobContainerName { get; set; }
        public string HubConnectionString { get; set; }
        public string DeviceId { get; set; }
    }
}

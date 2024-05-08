using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Interfaces
{
    public interface IHasKey
    {
        /// <summary>
        /// The item's FireBase database key.
        /// </summary>
        public string Key { get; set; }
    }
}

using ContainerFarmManagement.Config;
using ContainerFarmManagement.Models;
using ContainerFarmManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Repos
{
    public class DataRepo
    {
        private DatabaseService<Container> containerDB;
        private DatabaseService<Account> accountDB;

        public DatabaseService<Container> ContainerDB
        {
            get
            {
                return containerDB ??= new DatabaseService<Container>(AuthService.Client.User, nameof(Container), ResourceStrings.FireBase_DB_BaseUrl, nameof(Container));
            }
        }
        public DatabaseService<Account> AccountDB
        {
            get
            {
                return accountDB ??= new DatabaseService<Account>(AuthService.Client.User, nameof(Account), ResourceStrings.FireBase_DB_BaseUrl, nameof(Account));
            }
        }
    }
}
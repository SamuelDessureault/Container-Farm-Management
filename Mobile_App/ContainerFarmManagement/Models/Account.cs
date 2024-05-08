using ContainerFarmManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//STS technologies
//Semester 6 - 2023-04-27
// App Dev III
// Contains key information about the currently signed in user

namespace ContainerFarmManagement.Models
{
    public class Account : INotifyPropertyChanged, IHasKey
    {
        /// <summary>
        /// Enum of all account types.
        /// </summary>
        public enum AccountType 
        {
            [Description("owner")]
            OWNER,
            [Description("technician")]
            TECHNICIAN
        }

        /// <summary>
        /// The Account object's key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The account's username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The account's email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The account's type.
        /// </summary>
        public AccountType Type { get; set; }

        public Account(string key, string username, string email, AccountType type = AccountType.OWNER)
        {
            Key = key;
            Username = username;
            Email = email;
            Type = type;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

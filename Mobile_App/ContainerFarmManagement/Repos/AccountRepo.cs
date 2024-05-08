using ContainerFarmManagement.Models;
using ContainerFarmManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Repos
{
    public class AccountRepo
    {
        private readonly DatabaseService<Account> account_db = App.DataRepo.AccountDB;

        public ObservableCollection<Account> Accounts
        {
            get
            {
                return account_db.Items;
            }
        }

        public AccountRepo()
        {
            if (Accounts.Count == 0) { }
                //AddTestAccounts();
        }

        private void AddTestAccounts()
        {
            //RemoveAllAccounts().ConfigureAwait(false);
            account_db.AddItemAsync(new Account("", "TestTech", "test@test.test", Account.AccountType.TECHNICIAN)).ConfigureAwait(false);
            account_db.AddItemAsync(new Account("", "TestOwner", "tester@gmail.com", Account.AccountType.OWNER)).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a account to the database.
        /// </summary>
        /// <param name="account">The account object to be added.</param>
        public async Task AddAccount(Account account)
        {
            await account_db.AddItemAsync(account);
        }

        /// <summary>
        /// Removes a account from the database.
        /// </summary>
        /// <param name="account"></param>
        public async Task RemoveAccount(Account account)
        {
            await account_db.DeleteItemAsync(account);
        }

        public async Task RemoveAllAccounts()
        {
            foreach (Account account in Accounts)
            {
                await RemoveAccount(account);
            }
        }

        /// <summary>
        /// Updates a account int the database.
        /// </summary>
        /// <param name="account"></param>
        public async Task EditAccount(Account account)
        {
            await account_db.UpdateItemAsync(account);
        }

        /// <summary>
        /// Gets an account in the database from its key.
        /// </summary>
        /// <param name="key">The key of the account.</param>
        /// <returns>The account object.</returns>
        public async Task<Account> GetAccount(string key)
        {
            return Accounts.ToList().Find(c => c.Key == key);
        }

        /// <summary>
        /// Gets an account in the database from its key.
        /// </summary>
        /// <param name="email">The key of the account.</param>
        /// <returns>The account object.</returns>
        public async Task<Account> GetAccountByEmail(string email)
        {
            return Accounts.ToList().Find(c => c.Email == email);
        }
    }
}

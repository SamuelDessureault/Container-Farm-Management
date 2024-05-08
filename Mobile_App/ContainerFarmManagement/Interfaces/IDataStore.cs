using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFarmManagement.Interfaces
{
    public interface IDataStore<T>
    {
        /// <summary>
        /// Add the item to the database.
        /// </summary>
        /// <param name="item">The item to be added.</param>
        /// <returns>An asyncronous task that return true if the item was added to the database, false otherwise.</returns>
        Task<bool> AddItemAsync(T item); //Create operation

        /// <summary>
        /// Updates the item in the database.
        /// </summary>
        /// <param name="item">The item to be updated.</param>
        /// <returns>An asyncronous task that return true if the item was updated in the database, false otherwise.</returns>
        Task<bool> UpdateItemAsync(T item); //Update operation

        /// <summary>
        /// Deletes the item from the database.
        /// </summary>
        /// <param name="item">The item to be deleted.</param>
        /// <returns>An asyncronous task that return true if the item was deleted from the database, false otherwise.</returns>
        Task<bool> DeleteItemAsync(T item); //Delete operation

        /// <summary>
        /// Gets a list of all items of type T in the database.
        /// </summary>
        /// <param name="forceRefresh">True if the database needs to refresh before fetching all items, false otherwise.</param>
        /// <returns>An asyncronous task that returns an enumerable of all items of type T in the database.</returns>
        Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false); //Get all items from database

        /// <summary>
        /// The list of all items of type T in the database. Used for UI binding.
        /// </summary>
        public ObservableCollection<T> Items { get; } //Item to bind to the collection
    }
}
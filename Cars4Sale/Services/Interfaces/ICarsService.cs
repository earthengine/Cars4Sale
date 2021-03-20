using Cars4Sale.Models;
using System;
using System.Collections.Generic;

namespace Cars4Sale.Services.Interfaces
{
    public interface ICarsService
    {
        /// <summary>
        /// Get all cars.
        /// </summary>
        /// <returns>All cars in the system are returned.</returns>
        IEnumerable<Car> Get();
        /// <summary>
        /// Get the car with the given ID
        /// </summary>
        /// <param name="car_id">The car ID given</param>
        /// <returns>The car with the given ID</returns>
        (Car, ApiError?) Get(Guid car_id);
        /// <summary>
        /// Add a new car to the stock of the current dealer.
        /// </summary>
        /// <param name="current_client">The dealer to add a new car.</param>
        /// <param name="new_car">The data for the new car.</param>
        /// <returns>The new car added to the system.</returns>
        (Car, ApiError?) AddCar(ApiClient current_client, NewCar new_car);
        /// <summary>
        /// Remove a car from the stock of the current dealer.
        /// </summary>
        /// <param name="current_client">The dealer to remove a car from.</param>
        /// <param name="car_id">The car id to be removed.</param>
        /// <returns></returns>
        (Car, ApiError?) RemoveCar(ApiClient current_client, Guid car_id);
        /// <summary>
        /// Update the stock of a car for the current dealler.
        /// </summary>
        /// <param name="current_client">The dealer to update stock.</param>
        /// <param name="car_id">The id of the car to update stock.</param>
        /// <param name="new_stock">The new stock value.</param>
        /// <returns></returns>
        (int?, ApiError?) UpdateStock(ApiClient current_client, Guid car_id, int new_stock);
    }
}

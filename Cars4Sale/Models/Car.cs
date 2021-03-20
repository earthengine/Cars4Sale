using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Cars4Sale.Models
{
    public struct NewCar
    {
        /// <summary>
        /// The make of the car.
        /// </summary>
        /// <example>Carl Benz</example>
        public string Make { get; set; }
        /// <summary>
        /// The model of the car.
        /// </summary>
        /// <example>Benz Patent Motor</example>
        public string Model { get; set; }
        /// <summary>
        /// The year of the car has been built
        /// </summary>
        /// <example>1886</example>
        public int Year { get; set; }
        /// <summary>
        /// How much cars in stock.
        /// </summary>
        /// <example>1</example>
        public int Stock { get; set; }
    }
    public struct RemovedCar
    {
        /// <summary>
        /// The car that has been removed.
        /// </summary>
        public Car Removed { get; set; }
    }
    public struct UpdatedStock
    {
        /// <summary>
        /// The Car ID that has been updated
        /// </summary>
        /// <example>12345678-1234-1234-1234-123456789abc</example>
        public Guid Id { get; internal set; }
        /// <summary>
        /// The original stock value
        /// </summary>
        /// <example>20</example>
        public int? From { get; internal set; }
        /// <summary>
        /// The updated stock value
        /// </summary>
        /// <example>10</example>
        public int To { get; internal set; }
    }


    public class Car : IEquatable<Car>
    {
        /// <summary>
        /// The ID of the car in the system.
        /// </summary>
        /// <example>12345678-1234-1234-1234-123456789abc</example>
        public Guid Id { get; set; }
        /// <summary>
        /// The make of the car.
        /// </summary>
        /// <example>Toyota</example>
        public string Make { get; set; }
        /// <summary>
        /// The model of the car.
        /// </summary>
        /// <example>Corola</example>
        public string Model { get; set; }
        /// <summary>
        /// The year the car was built.
        /// </summary>
        /// <example>2015</example>
        public int Year { get; set; }
        /// <summary>
        /// How much cars belong to this dealer.
        /// </summary>
        /// <example>20</example>
        public int Stock { get; set; }
        /// <summary>
        /// Which dealer this car belongs to.
        /// </summary>
        [JsonIgnore]
        public ApiClient Client { get; set; }
        // This field is to avoid ABA update errors.
        //
        // That is, if you want to change the value from A to C,
        // and you only check that the object is still A when you
        // try to update, there is a chance that other threads
        // may successfully updated the value from A to B and back to A,
        // and you don't even notice it.
        [JsonIgnore]
        internal uint UpdateCount { get; set; } = 0;

        public bool Equals(Car other)
        {
            return Id == other.Id && Make == other.Make && Model == other.Model
                && Year == other.Year && Client.ApiKey == other.Client.ApiKey
                && UpdateCount == other.UpdateCount;
        }
    }

}

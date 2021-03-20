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

    public class Cars { 

        private static ConcurrentDictionary<Guid, Car> _Cars = new ConcurrentDictionary<Guid, Car>(new Dictionary<Guid, Car> {
            { new Guid("CAEACA2D-E407-49E3-8DDE-937A678B387B"), new Car {
                Id = new Guid("CAEACA2D-E407-49E3-8DDE-937A678B387B"),
                Make = "Toyota",
                Model = "Corola",
                Year = 2015,
                Stock = 20,
                Client = ApiClient.Clients.Values.ToArray()[0] } 
            },
            { new Guid("7632CBE9-0F16-4A30-9A17-FAEA4B4DE169"), new Car {
                Id = new Guid("7632CBE9-0F16-4A30-9A17-FAEA4B4DE169"),
                Make = "Carl Benz",
                Model = "Benz Patent Motor",
                Year = 1886,
                Stock = 1,
                Client = ApiClient.Clients.Values.ToArray()[1] }
            },
        });

        public static IEnumerable<Car> Get()
        {
            return _Cars.Values;
        }

        public static (Car, ApiError?) Get(Guid car_id)
        {
            if (_Cars.ContainsKey(car_id))
            {
                return (_Cars[car_id], null);
            }
            return (null, ApiError.NotFound($"Car {car_id} does not exist"));
        }

        public static (Car, ApiError?) AddCar(ApiClient current_client, NewCar new_car)
        {
            if (new_car.Year < 1886)
            {
                return (null, ApiError.BadRequest(
                    $@"The first gas powered vehicle was invented in 1886. Your car's built year {
                        new_car.Year} is earlier than that."
                ));
            }
            if (new_car.Year > DateTime.Now.Year)
            {
                return (null, ApiError.BadRequest(
                    $"Your car's built year {new_car.Year} is in the future."
                ));
            }

            var car = new Car
            {
                Client = current_client,
                Id = Guid.NewGuid(),
                Make = new_car.Make,
                Model = new_car.Model,
                Year = new_car.Year,
                Stock = new_car.Stock,
            };
            if(!_Cars.TryAdd(car.Id, car)) {
                // We don't know what's going on; this shouldn't happen
                return (null, ApiError.Internal($"Unexpected error: failed to add new car"));
            }
            return (car, null);
        }

        public static (Car, ApiError?) RemoveCar(ApiClient current_client, Guid car_id)
        {
            (var _, var maybe_error) = CheckAccess(current_client, car_id);
            if(maybe_error is ApiError error) { return (null, error); }

            if (!_Cars.TryRemove(car_id, out Car car))
            {
                return (null, ApiError.Gone($"Car {car_id} was already removed"));
            };
            return (car, null);
        }

        public static (int?, ApiError?) UpdateStock(ApiClient current_client, Guid car_id, int new_stock)
        {
            (var car, var maybe_error) = CheckAccess(current_client, car_id);
            if (maybe_error is ApiError error) { return (null, error); }

            // Always create a new object rather than updating the old
            // so there will be no partial updates in the car objects.
            var new_car = new Car {
                Make = car.Make, Model = car.Model, Year = car.Year, Stock = new_stock,
                Client = car.Client, Id = car.Id, UpdateCount = car.UpdateCount + 1
            };
            if(!_Cars.TryUpdate(car_id, new_car, car)) {
                return (null, ApiError.Conflict($"Car (ID {car_id}) has been updated by someone else"));
            }
            return (car.Stock, null);
        }

        private static (Car, ApiError?) CheckAccess(ApiClient current_client, Guid car_id)
        {
            return (!_Cars.TryGetValue(car_id, out Car car) || car.Client.ApiKey != current_client.ApiKey)
                ?  (null, ApiError.Forbidden($"Car (ID {car_id}) is not under control of ${current_client.DealerName} or it does not exist")) 
                :  (car, null);
        }
    }
}

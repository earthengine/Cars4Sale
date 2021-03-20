using Cars4Sale.Models;
using Cars4Sale.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cars4Sale.Services
{
    public class CarsService: IServiceProvider, ICarsService
    {
        private ConcurrentDictionary<Guid, Car> _Cars = new ConcurrentDictionary<Guid, Car>(new Dictionary<Guid, Car> {
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

        public IEnumerable<Car> Get()
        {
            return _Cars.Values;
        }

        public (Car, ApiError?) Get(Guid car_id)
        {
            if (_Cars.ContainsKey(car_id))
            {
                return (_Cars[car_id], null);
            }
            return (null, ApiError.NotFound($"Car {car_id} does not exist"));
        }

        public (Car, ApiError?) AddCar(ApiClient current_client, NewCar new_car)
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
            if (!_Cars.TryAdd(car.Id, car))
            {
                // We don't know what's going on; this shouldn't happen
                return (null, ApiError.Internal($"Unexpected error: failed to add new car"));
            }
            return (car, null);
        }

        public (Car, ApiError?) RemoveCar(ApiClient current_client, Guid car_id)
        {
            (var _, var maybe_error) = CheckAccess(current_client, car_id);
            if (maybe_error is ApiError error) { return (null, error); }

            if (!_Cars.TryRemove(car_id, out Car car))
            {
                return (null, ApiError.Gone($"Car {car_id} was already removed"));
            };
            return (car, null);
        }

        public (int?, ApiError?) UpdateStock(ApiClient current_client, Guid car_id, int new_stock)
        {
            (var car, var maybe_error) = CheckAccess(current_client, car_id);
            if (maybe_error is ApiError error) { return (null, error); }

            // Always create a new object rather than updating the old
            // so there will be no partial updates in the car objects.
            var new_car = new Car
            {
                Make = car.Make,
                Model = car.Model,
                Year = car.Year,
                Stock = new_stock,
                Client = car.Client,
                Id = car.Id,
                UpdateCount = car.UpdateCount + 1
            };
            if (!_Cars.TryUpdate(car_id, new_car, car))
            {
                return (null, ApiError.Conflict($"Car (ID {car_id}) has been updated by someone else"));
            }
            return (car.Stock, null);
        }

        private (Car, ApiError?) CheckAccess(ApiClient current_client, Guid car_id)
        {
            return (!_Cars.TryGetValue(car_id, out Car car) || car.Client.ApiKey != current_client.ApiKey)
                ? (null, ApiError.Forbidden($"Car (ID {car_id}) is not under control of ${current_client.DealerName} or it does not exist"))
                : (car, null);
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}

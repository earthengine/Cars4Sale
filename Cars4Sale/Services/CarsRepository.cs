using Cars4Sale.Models;
using Cars4Sale.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cars4Sale.Services
{
    public class CarsRepository : ICarsRepository
    {
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
            { new Guid("{087F739A-19CB-4951-B8A6-25A4794F492A}"), new Car {
                Id = new Guid("{087F739A-19CB-4951-B8A6-25A4794F492A}"),
                Make = "Audi",
                Model = "A4",
                Year = 2020,
                Stock = 15,
                Client = ApiClient.Clients.Values.ToArray()[1] }
            },
        });

        public IEnumerable<Car> Values => _Cars.Values;

        public bool ContainsKey(Guid car_id)
        {
            return _Cars.ContainsKey(car_id);
        }

        public Car Get(Guid car_id)
        {
            return _Cars[car_id];
        }

        public bool TryAdd(Guid car_id, Car car)
        {
            return _Cars.TryAdd(car_id, car);
        }

        public bool TryGetValue(Guid car_id, out Car car)
        {
            return _Cars.TryGetValue(car_id, out car);
        }

        public bool TryRemove(Guid car_id, out Car car)
        {
            return _Cars.TryRemove(car_id, out car);
        }

        public bool TryUpdate(Guid car_id, Car new_car, Car car)
        {
            return _Cars.TryUpdate(car_id, new_car, car);
        }
    }
}

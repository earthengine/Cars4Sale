using Cars4Sale.Models;
using System;
using System.Collections.Generic;

namespace Cars4Sale.Services.Interfaces
{
    public interface ICarsRepository
    {
        IEnumerable<Car> Values { get; }

        bool ContainsKey(Guid car_id);
        Car Get(Guid car_id);
        bool TryUpdate(Guid car_id, Car new_car, Car car);
        bool TryGetValue(Guid car_id, out Car car);
        bool TryAdd(Guid car_id, Car car);
        bool TryRemove(Guid car_id, out Car car);
    }
}

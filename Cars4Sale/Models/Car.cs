using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Cars4Sale.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Stock { get; set; }
        [JsonIgnore]
        public ApiClient Client { get; set; }
        [JsonIgnore]
        public bool Deleted { get; set; } = false;
    }

    public class Cars { 

        private static ConcurrentDictionary<int, Car> _Cars = new ConcurrentDictionary<int, Car>(new Dictionary<int, Car> {
            { 1, new Car { Id = 1, Make = "Toyota", Model = "Corola", Year = 2015, Stock = 20, Client = ApiClient.Clients[0] } }
        });

        public static ICollection<Car> Get()
        {
            return _Cars.Values;
        }

        public static void AddCar(Car car)
        {
            var cnt = _Cars.Count;
            car.Id = cnt;
            while(_Cars.GetOrAdd(cnt, car) != car) {
                cnt++;
                car.Id = cnt;
            }
        }

        public void RemoveCar(ApiClient current_client, int id)
        {
        }
    }
}

using System;
using System.Collections.Generic;

namespace Cars4Sale.Models
{
    public class ApiClient
    {
        public Guid ApiKey { get; set; }
        public string DealerName { get; set; }
        public static IReadOnlyDictionary<Guid, ApiClient> Clients = new Dictionary<Guid, ApiClient>
        {
            {new Guid("DDF53D65-E63B-4677-A0F5-DE3D8AE576AE"), new ApiClient { ApiKey = new Guid("DDF53D65-E63B-4677-A0F5-DE3D8AE576AE"), DealerName = "CarsRus" } },
            {new Guid("03C80182-D7D7-4732-92EC-3FCC898EB706"), new ApiClient { ApiKey = new Guid("03C80182-D7D7-4732-92EC-3FCC898EB706"), DealerName = "ECars" } }
        };
    }
}

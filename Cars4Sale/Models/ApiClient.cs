using System.Collections.Generic;

namespace Cars4Sale.Models
{
    public class ApiClient
    {
        public string ApiKey { get; set; }
        public static IList<ApiClient> Clients = new List<ApiClient>
        {
            new ApiClient { ApiKey = "DDF53D65-E63B-4677-A0F5-DE3D8AE576AE" },
            new ApiClient { ApiKey = "03C80182-D7D7-4732-92EC-3FCC898EB706" }
        }.AsReadOnly();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

using Cars4Sale.Models;

namespace Cars4Sale.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Cars4SaleController : ControllerBase
    {
        private readonly ILogger<Cars4SaleController> _logger;

        public Cars4SaleController(ILogger<Cars4SaleController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get all cars in a list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Car> Get()
        {
            return Cars.Get().ToArray();
        }
    }
}

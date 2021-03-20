using Cars4Sale.Attributes;
using Cars4Sale.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

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
        /// Get all cars in a list.
        /// </summary>
        /// <param name="model">The car model to search, optional.</param>
        /// <param name="make">The car make to search, optional.</param>
        /// <param name="all">Set to true to get all vehicle details including from other dealers; false to only return cars for the current dealer.</param>
        /// <returns>The list of cars</returns>
        [HttpGet]
        [ApiKey(Optional = true)]
        [ProducesResponseType(typeof(Car[]), StatusCodes.Status200OK)]
        public IActionResult GetList(string model = null, string make = null, bool all = false)
        {
            var current_client = HttpContext.Items["current_client"] as ApiClient;
            if (current_client == null) all = true;
            return Ok(Cars.Get()
                          .Where(x => model!=null ? x.Model.Contains(model) : true)
                          .Where(x => make != null ? x.Model.Contains(make) : true)
                          .Where(x => all ? true : x.Client == current_client));
        }

        /// <summary>
        /// Get information of a single car.
        /// </summary>
        /// <param name="car_id">The car's ID to get information from.</param>
        /// <returns>The list of cars</returns>
        /// <response code="404">There is no car with the specified ID exists in the system.</response>
        [HttpGet]
        [Route("{car_id}")]
        [ProducesResponseType(typeof(Car), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public IActionResult Get(Guid car_id)
        {
            return Ok(Cars.Get(car_id));
        }

        /// <summary>
        /// Add a car to the current API client.
        /// </summary>
        /// <param name="new_car">The new car data</param>
        /// <returns>The new car with the ID allocated.</returns>
        /// <response code="400">The car data is no valid. Check the resulting string for details.</response>
        /// <response code="500">Something weird happened that stops the system accepting the car data. Contact Cars4Sale for support.</response>
        [HttpPost]
        [ApiKey]
        [ProducesResponseType(typeof(Car), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult Add(NewCar new_car)
        {
            var current_client = HttpContext.Items["current_client"] as ApiClient;

            (var car, var maybe_error) = Cars.AddCar(current_client, new_car);
            if (maybe_error is ApiError error)
            {
                return error.ToContentResult();
            }
            return Ok(car);
        }

        /// <summary>
        /// Add a car to the current API client.
        /// </summary>
        /// <param name="car_id">The ID for the car to be removed</param>
        /// <returns>A JSON object includes the car that was removed</returns>
        /// <response code="403">Either the car you are going to remove is not belong to you the dealer, or the car does not exist.</response>
        /// <response code="410">The car you are trying to remove was removed already by another similar request.</response>
        [HttpDelete]
        [Route("{car_id}")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status410Gone)]
        public IActionResult Remove(Guid car_id)
        {
            var current_client = HttpContext.Items["current_client"] as ApiClient;

            var (car, maybe_error) = Cars.RemoveCar(current_client, car_id);
            if (maybe_error is ApiError error)
            {
                return error.ToContentResult();
            }
            return Ok(new { Removed = car });
        }

        /// <summary>
        /// Update the car stock value
        /// </summary>
        /// <param name="car_id">The ID for the car to be updated</param>
        /// <param name="new_stock">The new stock value to set</param>
        /// <returns>A JSON object includes the car ID, the old stock value and the new</returns>
        /// <response code="403">Either the car you are going to remove is not belong to you the dealer, or the car does not exist.</response>
        /// <response code="410">A similar request attempted to update the stock. Please try again.</response>
        [HttpPatch]
        [Route("{car_id}")]
        [ApiKey]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult UpdateStock(Guid car_id, [FromBody] int new_stock)
        {
            var current_client = HttpContext.Items["current_client"] as ApiClient;

            var (old_stock, maybe_error) = Cars.UpdateStock(current_client, car_id, new_stock);
            if (maybe_error is ApiError error)
            {
                return error.ToContentResult();
            }
            return Ok(new { Id = car_id, From = old_stock, To = new_stock });
        }
    }
}

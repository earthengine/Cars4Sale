using Cars4Sale.Attributes;
using Cars4Sale.Models;
using Cars4Sale.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Cars4Sale.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Cars4SaleController : ControllerBase
    {
        private readonly ICarsService _carService;

        public Cars4SaleController(ICarsService carService)
        {
            _carService = carService;
        }

        /// <summary>
        /// Get all cars in a list, can be filtered by model or make, and can limit the result to the current dealer only.
        /// </summary>
        /// <param name="make" example="Toyota">The car make to search, optional.</param>
        /// <param name="model" example="Corola">The car model to search, optional.</param>
        /// <param name="all" example="true">Set to true to get all vehicle details including from other dealers; false to only return cars for the current dealer.</param>
        /// <returns>The list of cars</returns>
        /// <response code="200">The result, a list of cars according to the filters.</response>
        [HttpGet]
        [ApiKey(Optional = true)]
        [ProducesResponseType(typeof(Car[]), StatusCodes.Status200OK)]
        public IActionResult GetList(string make = null, string model = null, bool all = false)
        {
            var current_client = HttpContext.Items["current_client"] as ApiClient;
            if (current_client == null) all = true;
            return Ok(_carService.Get().Where(x => 
                            (model == null || x.Model.Contains(model, StringComparison.InvariantCultureIgnoreCase)) &&
                            (make == null || x.Make.Contains(make, StringComparison.InvariantCultureIgnoreCase)) &&
                            (all || x.Client == current_client)));
        }

        /// <summary>
        /// Get information of a single car.
        /// </summary>
        /// <param name="car_id">The car's ID to get information from.</param>
        /// <returns>The list of cars</returns>
        /// <response code="200">The car's information</response>
        /// <response code="404" example="Car 12345678-1234-1234-1234-123456789abc does not exist">
        ///     There is no car with the specified ID exists in the system.
        /// </response>
        [HttpGet]
        [Route("{car_id}")]
        [ProducesResponseType(typeof(Car), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
        public IActionResult Get(Guid car_id)
        {
            (var car, var maybe_error) = _carService.Get(car_id);
            if (maybe_error is ApiError error)
            {
                return error.ToObjectResult();
            }
            return Ok(car);

        }

        /// <summary>
        /// Add a car to the current API client.
        /// </summary>
        /// <param name="new_car">The new car data</param>
        /// <returns>The new car with the ID allocated.</returns>
        /// <response code="200">The new car was just added.</response>
        /// <response code="400" example="Your car's built year 2100 is in the future.">
        ///     The car data is not valid. Check the resulting string for details.
        /// </response>
        /// <response code="500" example="Ouch... what happened?">
        ///     Something weird happened that stops the system accepting the car data. Contact Cars4Sale for support.
        /// </response>
        [HttpPost]
        [ApiKey]
        [ProducesResponseType(typeof(Car), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
        public IActionResult Add(NewCar new_car)
        {
            var current_client = HttpContext.Items["current_client"] as ApiClient;

            (var car, var maybe_error) = _carService.AddCar(current_client, new_car);
            if (maybe_error is ApiError error)
            {
                return error.ToObjectResult();
            }
            return Ok(car);
        }

        /// <summary>
        /// Add a car to the current API client.
        /// </summary>
        /// <param name="car_id" example="12345678-1234-1234-1234-123456789abc">The ID for the car to be removed.</param>
        /// <returns>A JSON object includes the car that was removed.</returns>
        /// <response code="200">The information of the car was just removed.</response>
        /// <response code="403" example="We have trouble with your car 12345678-1234-1234-1234-123456789abc...">
        ///     Either the car you are going to remove is not belong to you the dealer, or the car does not exist.
        /// </response>
        /// <response code="410" example="Someone else already removed this car. Don't worry.">
        ///     The car you are trying to remove was removed already by another similar request.
        /// </response>
        [HttpDelete]
        [ApiKey]
        [Route("{car_id}")]
        [ProducesResponseType(typeof(RemovedCar), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status410Gone)]
        public IActionResult Remove(Guid car_id)
        {
            var current_client = HttpContext.Items["current_client"] as ApiClient;

            var (car, maybe_error) = _carService.RemoveCar(current_client, car_id);
            if (maybe_error is ApiError error)
            {
                return error.ToObjectResult();
            }
            return Ok(new RemovedCar{ Removed = car });
        }

        /// <summary>
        /// Update the car stock value
        /// </summary>
        /// <param name="car_id">The ID for the car to be updated</param>
        /// <param name="new_stock">The new stock value to set</param>
        /// <returns>A JSON object includes the car ID, the old stock value and the new</returns>
        /// <response code="200">The car id, new and old stock value.</response>
        /// <response code="403" example="We have trouble with your car 12345678-1234-1234-1234-123456789abc...">
        ///     Either the car you are going to remove is not belong to you the dealer, or the car does not exist.
        /// </response>
        /// <response code="410" example="Someone on your side has updated it. You may need to review it.">
        ///     A similar request attempted to update the stock. Please try again.
        /// </response>
        [HttpPatch]
        [Route("{car_id}")]
        [ApiKey]
        [ProducesResponseType(typeof(UpdatedStock), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
        public IActionResult UpdateStock(Guid car_id, [FromBody] int new_stock)
        {
            var current_client = HttpContext.Items["current_client"] as ApiClient;

            var (old_stock, maybe_error) = _carService.UpdateStock(current_client, car_id, new_stock);
            if (maybe_error is ApiError error)
            {
                return error.ToObjectResult();
            }
            return Ok(new UpdatedStock{ Id = car_id, From = old_stock, To = new_stock });
        }
    }
}

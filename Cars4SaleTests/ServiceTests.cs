using NUnit.Framework;
using Cars4Sale.Models;
using Moq;
using Cars4Sale.Services.Interfaces;
using System.Collections.Generic;
using Cars4Sale.Services;
using System.Linq;
using System;
using Cars4Sale;

namespace Cars4SaleTests
{
    public class ServiceTests
    {
        private SimpleInjector.Container container;

        [SetUp]
        public void Setup()
        {
        }

        private static readonly NewCar[] NewCars =
        {
            new NewCar { Make = "Make 1", Model = "Model 1", Year=1883, Stock = 2 },
            new NewCar { Make = "Make 2", Model = "Model 2", Year=DateTime.Now.Year+3, Stock = 3 }
        };

        private static readonly ApiClient[] Clients =
        {
            new ApiClient { ApiKey = Guid.NewGuid(), DealerName ="Dealer 1" },
            new ApiClient { ApiKey = Guid.NewGuid(), DealerName ="Dealer 2" },
            new ApiClient { ApiKey = Guid.NewGuid(), DealerName ="Dealer 3" },
            new ApiClient { ApiKey = Guid.NewGuid(), DealerName ="Dealer 4" }
        };

        private static readonly Car[] Cars =
        {
            new Car { Id = Guid.NewGuid(), Client=Clients[0], Make = "Make 1", Model = "Model 1", Year=2015, Stock=20 },
            new Car { Id = Guid.NewGuid(), Client=Clients[1], Make = "Make 2", Model = "Model 2", Year=2016, Stock=30 },
            new Car { Id = Guid.NewGuid(), Client=Clients[2], Make = "Make 3", Model = "Model 3", Year=2017, Stock=40 },
            new Car { Id = Guid.NewGuid(), Client=Clients[3], Make = "Make 4", Model = "Model 4", Year=2018, Stock=50 }
        };

        [Test]
        public void CanGetAllCars()
        {
            var cars = new List<Car>();

            var mock_repository = new Mock<ICarsRepository>();
            mock_repository.Setup(v => v.Values).Returns(cars);

            var cars_service = new CarsService(mock_repository.Object);

            Assert.That(cars_service.Get(), Is.Empty);

            cars.Add(Cars[0]);
            var result = cars_service.Get().ToArray();
            Assert.That(result, Is.EqualTo(new Car[] { cars[0] }));

            cars.Add(Cars[1]);
            result = cars_service.Get().ToArray();
            Assert.That(result, Is.EqualTo(new Car[] { cars[0], cars[1] }));
        }

        [Test]
        public void CanGetCarById()
        {
            var mock_repository = new Mock<ICarsRepository>();
            mock_repository.Setup(v => v.ContainsKey(It.IsAny<Guid>()))
                           .Returns((Guid guid) => Cars.Any(c => c.Id == guid));
            mock_repository.Setup(v => v.Get(It.IsAny<Guid>()))
                           .Returns((Guid guid) => Cars.Where(c => c.Id == guid).First());


            var cars_service = new CarsService(mock_repository.Object);

            (var car, var maybe_error) = cars_service.Get(Cars[0].Id);
            Assert.That(car, Is.SameAs(Cars[0]));
            Assert.That(!(maybe_error is ApiError));

            (car, maybe_error) = cars_service.Get(Cars[1].Id);
            Assert.That(car, Is.SameAs(Cars[1]));
            Assert.That(maybe_error, Is.Null);

            (car, maybe_error) = cars_service.Get(Guid.NewGuid());
            Assert.That(car, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("Car .* does not exist"));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public void CanAddCar()
        {
            var called = false;
            var expected_try_add_result = false;

            var mock_repository = new Mock<ICarsRepository>();
            mock_repository.Setup(v => v.TryAdd(It.IsAny<Guid>(), It.IsAny<Car>()))
                           .Returns((Guid _g, Car _c) => { called = true; return expected_try_add_result; });

            var cars_service = new CarsService(mock_repository.Object);

            (var car, var maybe_error) = cars_service.AddCar(Clients[0], new NewCar { Year = 1885 });
            Assert.That(!called);
            Assert.That(car, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("1886. .* is earlier than that"));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(400));

            (car, maybe_error) = cars_service.AddCar(Clients[1], new NewCar { Year = DateTime.Now.Year + 1 });
            Assert.That(!called);
            Assert.That(car, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("Your car's built year .* is in the future."));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(400));

            (car, maybe_error) = cars_service.AddCar(Clients[2], new NewCar { Year = DateTime.Now.Year - 1 });
            Assert.That(called);
            Assert.That(car, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("Unexpected error: failed to add new car"));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(500));

            called = false;
            expected_try_add_result = true;
            (car, maybe_error) = cars_service.AddCar(Clients[3], new NewCar
            {
                Year = DateTime.Now.Year - 1,
                Model = "Model 1",
                Make = "Make 1",
                Stock = 20
            });
            Assert.That(called);
            Assert.That(car.Model, Is.EqualTo("Model 1"));
            Assert.That(car.Make, Is.EqualTo("Make 1"));
            Assert.That(car.Stock, Is.EqualTo(20));
            Assert.That(car.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(Cars.Where(c => c.Id == car.Id), Is.Empty);
            Assert.That(maybe_error, Is.Null);
        }

        [Test]
        public void CanRemoveCar()
        {
            var car_to_get = Cars[0];
            var car_to_remove = Cars[0];
            var try_get_value_called = false;
            var try_remove_called = false;
            var expected_try_get_value_result = false;
            var expected_try_remove_result = false;

            var mock_repository = new Mock<ICarsRepository>();
            mock_repository.Setup(v => v.TryGetValue(It.IsAny<Guid>(), out car_to_get))
                           .Returns((Guid _g, Car _c) => { try_get_value_called = true; return expected_try_get_value_result; });
            mock_repository.Setup(v => v.TryRemove(It.IsAny<Guid>(), out car_to_remove))
                           .Returns((Guid _g, Car _c) => { try_remove_called = true; return expected_try_remove_result; });

            var cars_service = new CarsService(mock_repository.Object);

            (var car, var maybe_error) = cars_service.RemoveCar(Clients[0], Cars[1].Id);
            Assert.That(try_get_value_called);
            Assert.That(!try_remove_called);
            Assert.That(car, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("not under control .* does not exist"));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(403));

            try_get_value_called = false;
            (car, maybe_error) = cars_service.RemoveCar(Clients[0], Guid.NewGuid());
            Assert.That(try_get_value_called);
            Assert.That(!try_remove_called);
            Assert.That(car, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("not under control .* does not exist"));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(403));

            try_get_value_called = false;
            expected_try_get_value_result = true;
            (car, maybe_error) = cars_service.RemoveCar(Clients[0], Cars[0].Id);
            Assert.That(try_get_value_called);
            Assert.That(try_remove_called);
            Assert.That(car, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("Car .* was already removed"));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(410));

            try_get_value_called = false;
            expected_try_get_value_result = true;
            try_remove_called = false;
            expected_try_remove_result = true;
            (car, maybe_error) = cars_service.RemoveCar(Clients[0], Cars[0].Id);
            Assert.That(try_get_value_called);
            Assert.That(try_remove_called);
            Assert.That(maybe_error, Is.Null);
            Assert.That(car, Is.SameAs(Cars[0]));
        }

        [Test]
        public void CanUpdateStock()
        {
            var car_to_get = Cars[0];
            var try_get_value_called = false;
            var expected_try_get_value_result = false;
            var try_update_called = false;
            var expected_try_update_result = false;

            var mock_repository = new Mock<ICarsRepository>();
            mock_repository.Setup(v => v.TryGetValue(It.IsAny<Guid>(), out car_to_get))
                           .Returns((Guid _g, Car _c) => { try_get_value_called = true; return expected_try_get_value_result; });
            mock_repository.Setup(v => v.TryUpdate(It.IsAny<Guid>(), It.IsAny<Car>(), It.IsAny<Car>()))
                           .Returns((Guid _g, Car _c1, Car _c2) => { try_update_called = true; return expected_try_update_result; });

            var cars_service = new CarsService(mock_repository.Object);

            (var maybe_updated, var maybe_error) = cars_service.UpdateStock(Clients[0], Cars[1].Id, 21);
            Assert.That(try_get_value_called);
            Assert.That(!try_update_called);
            Assert.That(maybe_updated, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("not under control .* does not exist"));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(403));

            try_get_value_called = false;
            (maybe_updated, maybe_error) = cars_service.UpdateStock(Clients[0], Guid.NewGuid(), 22);
            Assert.That(try_get_value_called);
            Assert.That(!try_update_called);
            Assert.That(maybe_updated, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("not under control .* does not exist"));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(403));

            try_get_value_called = false;
            expected_try_get_value_result = true;
            (maybe_updated, maybe_error) = cars_service.UpdateStock(Clients[0], Guid.NewGuid(), 23);
            Assert.That(try_get_value_called);
            Assert.That(try_update_called);
            Assert.That(maybe_updated, Is.Null);
            Assert.That(maybe_error?.Reason, Does.Match("Car .* has been updated"));
            Assert.That(maybe_error?.StatusCode, Is.EqualTo(409));

            try_get_value_called = false;
            expected_try_get_value_result = true;
            try_update_called = false;
            expected_try_update_result = true;
            (maybe_updated, maybe_error) = cars_service.UpdateStock(Clients[0], Guid.NewGuid(), 24);
            Assert.That(try_get_value_called);
            Assert.That(try_update_called);
            Assert.That(maybe_updated, Is.EqualTo(Cars[0].Stock));
            Assert.That(maybe_error, Is.Null);
        }
    }
}
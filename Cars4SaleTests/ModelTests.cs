using NUnit.Framework;
using Cars4Sale.Models;

namespace Cars4SaleTests
{
    public class ModelTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanCreateCar()
        {
            var car = new Car();
            Assert.Pass();
        }
    }
}
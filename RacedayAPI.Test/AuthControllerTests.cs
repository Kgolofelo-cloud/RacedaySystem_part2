using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RacedayAPI.Controllers;
using RacedayAPI.Data;
using RacedayAPI.Requests;
using Xunit;

namespace RacedayAPI.Tests
{
    public class AuthControllerTests
    {
        // Helper to create a fresh In-Memory database for every test
        private DbContextOptions<RaceDayDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        // Helper to mock the HTTP Context and Session
        private ControllerContext GetMockedContext()
        {
            var mockSession = new Mock<ISession>();
            var httpContext = new DefaultHttpContext { Session = mockSession.Object };
            return new ControllerContext { HttpContext = httpContext };
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsCreated()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new RaceDayDbContext(options);
            var controller = new AuthController(context)
            {
                ControllerContext = GetMockedContext()
            };

            var request = new RegisterRequest
            {
                FirstName = "Thabo",
                LastName = "Mokoena",
                Email = "thabo@test.com",
                Password = "password123",
                Role = "Participant"
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);
        }

        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            // Arrange
            var options = GetInMemoryOptions();
            using var context = new RaceDayDbContext(options);
            var controller = new AuthController(context)
            {
                ControllerContext = GetMockedContext()
            };

            var request = new RegisterRequest
            {
                FirstName = "Hacker",
                LastName = "Man",
                Email = "hacker@test.com",
                Password = "password123",
                Role = "Admin" // Not allowed by the system
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }
    }
}
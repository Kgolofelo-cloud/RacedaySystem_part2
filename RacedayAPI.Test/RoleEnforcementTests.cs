using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using Moq;

using RacedayAPI.Controllers;

using RacedayAPI.Data;

using RacedayAPI.Models;

using RacedayAPI.Requests;

using System.Text;

using Xunit;

namespace RacedayAPI.Tests

{

    public class RoleEnforcementTests

    {

        private DbContextOptions<RaceDayDbContext> GetInMemoryOptions()

        {

            return new DbContextOptionsBuilder<RaceDayDbContext>()

                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())

                .Options;

        }

        // Helper to safely fake the Session Cookie for specific roles

        private ControllerContext GetMockedContext(int userId, string role)

        {

            var mockSession = new Mock<ISession>();

            // ASP.NET Core's SetInt32 uses a specific byte layout, this safely mocks it

            var userIdBytes = new byte[] { (byte)(userId >> 24), (byte)(userId >> 16), (byte)(userId >> 8), (byte)userId };

            mockSession.Setup(s => s.TryGetValue("UserId", out userIdBytes)).Returns(true);

            var roleBytes = Encoding.UTF8.GetBytes(role);

            mockSession.Setup(s => s.TryGetValue("Role", out roleBytes)).Returns(true);

            return new ControllerContext

            {

                HttpContext = new DefaultHttpContext { Session = mockSession.Object }

            };

        }

        [Fact]

        public async Task CreateEvent_AsOrganiser_ReturnsCreated()

        {

            // Arrange

            var options = GetInMemoryOptions();

            using var context = new RaceDayDbContext(options);

            var controller = new EventsController(context)

            {

                ControllerContext = GetMockedContext(userId: 1, role: "Organiser")

            };

            var request = new EventRequest { EventName = "Organiser Event", EventDate = DateTime.Now, LocationID = 1 };

            // Act

            var result = await controller.CreateEvent(request);

            // Assert

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, objectResult.StatusCode);

        }

        [Fact]

        public async Task CreateEvent_AsParticipant_ReturnsForbidden()

        {

            // Arrange

            var options = GetInMemoryOptions();

            using var context = new RaceDayDbContext(options);

            var controller = new EventsController(context)

            {

                ControllerContext = GetMockedContext(userId: 2, role: "Participant")

            };

            var request = new EventRequest { EventName = "Hacked Event", EventDate = DateTime.Now, LocationID = 1 };

            // Act

            var result = await controller.CreateEvent(request);

            // Assert

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(403, objectResult.StatusCode); // 403 Forbidden proves role rejection works

        }

        [Fact]

        public async Task Enrol_AsParticipant_ReturnsCreated()

        {

            // Arrange

            var options = GetInMemoryOptions();

            using var context = new RaceDayDbContext(options);

            // We must add a mock category to the InMemory DB so the enrolment has something to link to

            context.RaceCategories.Add(new RaceCategory { CategoryID = 1, EventID = 1, CategoryName = "5km", Distance = 5, EntryFee = 100 });

            await context.SaveChangesAsync();

            var controller = new RegistrationsController(context)

            {

                ControllerContext = GetMockedContext(userId: 2, role: "Participant")

            };

            var request = new RegistrationRequest { CategoryID = 1 };

            // Act

            var result = await controller.Enrol(request);

            // Assert

            var objectResult = Assert.IsType<ObjectResult>(result);

            Assert.Equal(201, objectResult.StatusCode);

        }

    }

}

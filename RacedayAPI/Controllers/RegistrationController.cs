using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacedayAPI.Data;
using RacedayAPI.Models;
using RacedayAPI.Requests;

namespace RacedayAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RegistrationsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public RegistrationsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/v1/registrations
        [HttpPost]
        public async Task<IActionResult> Enrol([FromBody] RegistrationRequest request)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Participant")
                return StatusCode(403, new { Message = "Forbidden: Not a participant." });

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            if (!await _context.RaceCategories.AnyAsync(c => c.CategoryID == request.CategoryID))
                return NotFound(new { Message = "Category ID invalid." });

            var registration = new Registration
            {
                UserID = userId,
                CategoryID = request.CategoryID,
                RegisteredAt = DateTime.Now
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { Message = "Registration successful." });
        }

        // GET: api/v1/registrations/my-enrolments
        [HttpGet("my-enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Participant")
                return StatusCode(403, new { Message = "Forbidden: Not a participant." });

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var enrolments = await _context.Registrations
                .Include(r => r.RaceCategory)
                .ThenInclude(c => c.Event)
                .Where(r => r.UserID == userId)
                .Select(r => new {
                    r.RegistrationID,
                    EventName = r.RaceCategory.Event.EventName,
                    Category = r.RaceCategory.CategoryName,
                    r.RegisteredAt
                })
                .ToListAsync();

            if (!enrolments.Any())
                return NotFound(new { Message = "No enrolments found." });

            return Ok(enrolments);
        }

        // GET: api/v1/registrations/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { Message = "Forbidden: Not an organiser." });

            var organiserId = HttpContext.Session.GetInt32("UserId").Value;
            var raceEvent = await _context.Events.FindAsync(eventId);

            if (raceEvent == null || raceEvent.OrganiserID != organiserId)
                return StatusCode(403, new { Message = "Forbidden: You do not manage this event." });

            var enrolments = await _context.Registrations
                .Include(r => r.User)
                .Include(r => r.RaceCategory)
                .Where(r => r.RaceCategory.EventID == eventId)
                .Select(r => new {
                    r.RegistrationID,
                    Participant = r.User.FirstName + " " + r.User.LastName,
                    Category = r.RaceCategory.CategoryName
                })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacedayAPI.Data;
using RacedayAPI.Models;
using RacedayAPI.Requests;

namespace RacedayAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/v1/events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            // Any logged-in user can view events
            if (HttpContext.Session.GetInt32("UserId") == null)
                return Unauthorized(new { Message = "Token missing/invalid. Please log in." });

            var events = await _context.Events
                .Include(e => e.Location)
                .Include(e => e.RaceCategories)
                .Select(e => new {
                    e.EventID,
                    e.EventName,
                    e.EventDate,
                    Location = e.Location.VenueName + ", " + e.Location.City,
                    Categories = e.RaceCategories.Select(c => new { c.CategoryID, c.CategoryName, c.Distance, c.EntryFee })
                }).ToListAsync();

            return Ok(events);
        }

        // POST: api/v1/events
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventRequest request)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { Message = "Forbidden: Not an organiser." });

            var organiserId = HttpContext.Session.GetInt32("UserId").Value;

            var newEvent = new Event
            {
                EventName = request.EventName,
                EventDate = request.EventDate,
                LocationID = request.LocationID,
                OrganiserID = organiserId
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { Message = "Event created.", EventID = newEvent.EventID });
        }

        // POST: api/v1/events/{id}/categories
        [HttpPost("{id}/categories")]
        public async Task<IActionResult> AddCategory(int id, [FromBody] CategoryRequest request)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { Message = "Forbidden: Not an organiser." });

            var organiserId = HttpContext.Session.GetInt32("UserId").Value;
            var raceEvent = await _context.Events.FindAsync(id);

            if (raceEvent == null)
                return NotFound(new { Message = "Event ID invalid." });
            // Optional: Ensure the organiser owns this event
            if (raceEvent.OrganiserID != organiserId)
                return StatusCode(403, new { Message = "Forbidden: You do not manage this event." });

            var newCategory = new RaceCategory
            {
                EventID = id,
                CategoryName = request.CategoryName,
                Distance = request.Distance,
                EntryFee = request.EntryFee
            };

            _context.RaceCategories.Add(newCategory);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { Message = "Category added." });
        }
    }
}
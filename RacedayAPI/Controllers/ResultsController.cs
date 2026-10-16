using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

using RacedayAPI.Data;

using RacedayAPI.Models;

using RacedayAPI.Requests;

namespace RacedayAPI.Controllers

{

    [Route("api/v1/[controller]")]

    [ApiController]

    public class ResultsController : ControllerBase

    {

        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)

        {

            _context = context;

        }

        // POST: api/v1/results

        [HttpPost]

        public async Task<IActionResult> CaptureResult([FromBody] ResultRequest request)

        {

            var role = HttpContext.Session.GetString("Role");

            if (role != "Organiser")

                return StatusCode(403, new { Message = "Forbidden: Not an organiser." });

            if (!TimeSpan.TryParse(request.FinishTime, out TimeSpan parsedTime))

                return BadRequest(new { Message = "Invalid time format. Use HH:MM:SS" });

            var registration = await _context.Registrations

                .Include(r => r.RaceCategory)

                .ThenInclude(c => c.Event)

                .FirstOrDefaultAsync(r => r.RegistrationID == request.RegistrationID);

            if (registration == null)

                return NotFound(new { Message = "Registration not found." });

            var organiserId = HttpContext.Session.GetInt32("UserId").Value;

            if (registration.RaceCategory.Event.OrganiserID != organiserId)

                return StatusCode(403, new { Message = "Forbidden: You do not manage this event." });

            var result = new RaceResult

            {

                RegistrationID = request.RegistrationID,

                FinishTime = parsedTime,

                OverallPosition = request.OverallPosition

            };

            _context.RaceResults.Add(result);

            await _context.SaveChangesAsync();

            return StatusCode(201, new { Message = "Result saved." });

        }

        // GET: api/v1/results/my-results

        [HttpGet("my-results")]

        public async Task<IActionResult> GetMyResults()

        {

            var role = HttpContext.Session.GetString("Role");

            if (role != "Participant")

                return StatusCode(403, new { Message = "Forbidden: Not a participant." });

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var results = await _context.RaceResults

                .Include(rr => rr.Registration)

                .ThenInclude(r => r.RaceCategory)

                .ThenInclude(c => c.Event)

                .Where(rr => rr.Registration.UserID == userId)

                .Select(rr => new {

                    Event = rr.Registration.RaceCategory.Event.EventName,

                    Category = rr.Registration.RaceCategory.CategoryName,

                    FinishTime = rr.FinishTime.ToString(@"hh\:mm\:ss"),

                    Position = rr.OverallPosition

                })

                .ToListAsync();

            return Ok(results);

        }

    }

}

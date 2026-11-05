using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacedayAPI.Data;
using RacedayAPI.Models;
using RacedayAPI.Requests;
using Azure.Storage.Blobs;

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

        // POST: api/v1/events/{id}/banner
        [HttpPost("{id}/banner")]
        public async Task<IActionResult> UploadEventBanner(int id, IFormFile file)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Organiser")
                return StatusCode(403, new { Message = "Forbidden: Not an organiser." });

            var raceEvent = await _context.Events.FindAsync(id);
            if (raceEvent == null)
                return NotFound(new { Message = "Event not found." });

            // Only the organiser who created the event can upload a banner
            var organiserId = HttpContext.Session.GetInt32("UserId").Value;
            if (raceEvent.OrganiserID != organiserId)
                return StatusCode(403, new { Message = "Forbidden: You do not manage this event." });

            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "No file uploaded." });

            // Connect to Azure Blob Storage (Using local Azurite emulator for development)
            string connectionString = "UseDevelopmentStorage=true";
            var blobServiceClient = new BlobServiceClient(connectionString);
            // Create a container named "event-banners" if it doesn't exist
            var containerClient = blobServiceClient.GetBlobContainerClient("event-banners");
            await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            // Generate a unique filename and upload
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = containerClient.GetBlobClient(uniqueFileName);
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            // Save the Azure URL to the database
            raceEvent.BannerUrl = blobClient.Uri.ToString();
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Banner uploaded successfully.", BannerUrl = raceEvent.BannerUrl });
        }

    }

}
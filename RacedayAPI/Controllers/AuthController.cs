using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RacedayAPI.Data;
using RacedayAPI.Models;
using RacedayAPI.Requests;

namespace RacedayAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // 1. Check if email exists
            if (await _context.AppUsers.AnyAsync(u => u.Email == request.Email))
                return Conflict(new { Message = "Email is already registered." });

            // 2. Validate Role
            if (request.Role != "Organiser" && request.Role != "Participant")
                return BadRequest(new { Message = "Role must be exactly 'Organiser' or 'Participant'." });

            // 3. Create User & Hash Password
            var user = new AppUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { Message = "Account created successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.AppUsers.SingleOrDefaultAsync(u => u.Email == request.Email);

            // Verify User & Password
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { Message = "Invalid email or password." });

            // Store ID and Role securely in Server Session
            HttpContext.Session.SetInt32("UserId", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok(new { Message = "Login successful", Role = user.Role });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { Message = "Logged out successfully." });
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { Message = "You are not logged in." });

            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
        }
    }
}
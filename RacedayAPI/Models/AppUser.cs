using RacedayAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace RacedayAPI.Models

{

    public class AppUser

    {

        [Key]

        public int UserID { get; set; }

        [Required, MaxLength(100)]

        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]

        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(150)]

        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]

        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]

        public string Role { get; set; } = string.Empty;

        public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    }

}

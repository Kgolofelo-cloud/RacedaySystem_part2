using RacedayAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace RacedayAPI.Models
{
    public class EventLocation
    {
        [Key]
        public int LocationID { get; set; }

        [Required, MaxLength(100)]
        public string Province { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string VenueName { get; set; } = string.Empty;

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
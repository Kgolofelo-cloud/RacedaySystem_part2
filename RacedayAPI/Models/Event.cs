using RacedayAPI.Models;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace RacedayAPI.Models

{

    public class Event

    {

        [Key]

        public int EventID { get; set; }

        [Required, MaxLength(150)]

        public string EventName { get; set; } = string.Empty;

        [Required]

        public DateTime EventDate { get; set; }

        [Required]

        public int OrganiserID { get; set; }

        [Required]

        public int LocationID { get; set; }

        [ForeignKey("OrganiserID")]

        public AppUser? Organiser { get; set; }

        [ForeignKey("LocationID")]

        public EventLocation? Location { get; set; }

        public ICollection<RaceCategory> RaceCategories { get; set; } = new List<RaceCategory>();

    }

}

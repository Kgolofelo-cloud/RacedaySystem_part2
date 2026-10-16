using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models

{

    public class Registration

    {

        [Key]

        public int RegistrationID { get; set; }

        [Required]

        public int UserID { get; set; }

        [Required]

        public int CategoryID { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        // Navigation Properties

        [ForeignKey("UserID")]

        public AppUser? User { get; set; }

        [ForeignKey("CategoryID")]

        public RaceCategory? RaceCategory { get; set; }

        public RaceResult? RaceResult { get; set; }

    }

}

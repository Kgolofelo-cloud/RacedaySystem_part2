using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models

{

    public class RaceResult

    {

        [Key]

        public int ResultID { get; set; }

        [Required]

        public int RegistrationID { get; set; }

        [Required]

        public TimeSpan FinishTime { get; set; }

        [Required]

        public int OverallPosition { get; set; }

        // Navigation Property

        [ForeignKey("RegistrationID")]

        public Registration? Registration { get; set; }

    }

}

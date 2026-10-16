using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace RacedayAPI.Models

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

        [ForeignKey("RegistrationID")]

        public Registration? Registration { get; set; }

    }

}

using System.ComponentModel.DataAnnotations;

namespace RacedayAPI.Requests
{
    public class ResultRequest
    {
        [Required]
        public int RegistrationID { get; set; }

        [Required]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Invalid time format. Use HH:MM:SS")]
        public string FinishTime { get; set; } = string.Empty;

        [Required]
        public int OverallPosition { get; set; }
    }
}
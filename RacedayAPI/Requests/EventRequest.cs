using System.ComponentModel.DataAnnotations;

namespace RacedayAPI.Requests
{
    public class EventRequest
    {
        [Required, MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public int LocationID { get; set; }
    }
}
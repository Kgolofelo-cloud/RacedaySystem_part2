using System.ComponentModel.DataAnnotations;

namespace RacedayAPI.Requests
{
    public class RegistrationRequest
    {
        [Required]
        public int CategoryID { get; set; }
    }
}
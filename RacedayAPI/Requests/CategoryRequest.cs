using System.ComponentModel.DataAnnotations;

namespace RacedayAPI.Requests
{
    public class CategoryRequest
    {
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        public decimal Distance { get; set; }

        [Required]
        public decimal EntryFee { get; set; }
    }
}
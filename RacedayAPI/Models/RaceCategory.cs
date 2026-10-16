using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayAPI.Models
{
    public class RaceCategory
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Distance { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal EntryFee { get; set; } = 0.00m;

        // Navigation Properties
        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
    }
}
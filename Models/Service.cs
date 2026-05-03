using System.ComponentModel.DataAnnotations;

namespace MakeupBookingAPI.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        [Range(1, 10000)] 
        public decimal Price { get; set; }

        [Required]
        [Range(1, 600)] 
        public int DurationMinutes { get; set; }
    }
}
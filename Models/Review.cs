using System;
using System.ComponentModel.DataAnnotations;

namespace MakeupBookingAPI.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string? Name { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [Required, MaxLength(1000)]
        public string? Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
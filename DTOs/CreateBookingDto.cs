using System;
using System.ComponentModel.DataAnnotations;

namespace MakeupBookingAPI.DTOs
{
    public class CreateBookingDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ServiceId { get; set; }

        public string Phone { get; set; }
    }
}

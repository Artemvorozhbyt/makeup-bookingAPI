using System.ComponentModel.DataAnnotations;

namespace MakeupBookingAPI.Models
{
    public class RegisterDto
    {
        [Required, EmailAddress, MaxLength(100)]
        public string? Email { get; set; }

        [Required, MinLength(6), MaxLength(100)]
        public string? Password { get; set; }

        [MaxLength(20)]
        public string Role { get; set; } = "User";
    }
}
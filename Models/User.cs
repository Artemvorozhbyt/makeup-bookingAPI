using System.ComponentModel.DataAnnotations;

namespace MakeupBookingAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string? Email { get; set; }

        [Required, MaxLength(255)]
        public string? PasswordHash { get; set; }

        [Required, MaxLength(20)]
        public string Role { get; set; } = "User";
    }
}
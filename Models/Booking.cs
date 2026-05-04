using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MakeupBookingAPI.Models;
using System.ComponentModel.DataAnnotations;

public class Booking
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    public string Phone { get; set; }

    [Required]
    public DateTime Date { get; set; } 

    [Required]
    public int ServiceId { get; set; }

    [Required]
    public int DurationMinutes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ServiceId))]
    public Service? Service { get; set; }
}
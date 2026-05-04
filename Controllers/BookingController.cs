using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MakeupBookingAPI.Data;
using MakeupBookingAPI.Models;
using MakeupBookingAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Linq;

namespace MakeupBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public BookingController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private readonly List<TimeSpan> allowedSlots = new()
        {
            new TimeSpan(09, 0, 0),
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            new TimeSpan(12, 0, 0),
            new TimeSpan(13, 0, 0),
            new TimeSpan(14, 0, 0),
            new TimeSpan(15, 0, 0),
            new TimeSpan(16, 0, 0),
            new TimeSpan(17, 0, 0),
            new TimeSpan(18, 0, 0),
        };

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
        {
            if (dto == null)
                return BadRequest("Невірні дані");

            if (string.IsNullOrWhiteSpace(dto.Phone) || dto.Phone.Length < 9)
                return BadRequest("Невірний номер телефону");

            if (dto.Date < DateTime.Now) 
                return BadRequest("Дата не може бути в минулому");

            if (string.IsNullOrWhiteSpace(dto.Email) || !IsValidEmail(dto.Email))
                return BadRequest("Невірний email");

            var service = await _context.Services.FindAsync(dto.ServiceId);
            if (service == null)
                return BadRequest("Послуга не знайдена");

            var localTime = dto.Date.TimeOfDay;

            if (!allowedSlots.Contains(localTime))
                return BadRequest("Недоступний час");

            var utcDate = DateTime.SpecifyKind(dto.Date, DateTimeKind.Local)
                                  .ToUniversalTime();

            int duration = service.DurationMinutes;

            var newStart = utcDate;
            var newEnd = utcDate.AddMinutes(duration);

            var start = utcDate.Date;
            var end = start.AddDays(1);

            var bookings = await _context.Bookings
                .Where(b => b.ServiceId == dto.ServiceId &&
                            b.Date >= start && b.Date < end)
                .ToListAsync();

            bool exists = bookings.Any(b =>
            {
                var existingStart = b.Date;
                var existingEnd = b.Date.AddMinutes(b.DurationMinutes);

                return newStart < existingEnd && newEnd > existingStart;
            });

            if (exists)
                return Conflict("Час зайнятий");

            var booking = new Booking
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Date = utcDate, 
                ServiceId = dto.ServiceId,
                DurationMinutes = duration,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            try
            {
                Console.WriteLine("SENDING EMAIL...");

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendBookingConfirmation(
                            booking.Email,
                            booking.Name,
                            service.Name,
                            booking.Date.ToLocalTime()
                        );

                        await _emailService.SendAdminNotification(
                            booking.Name,
                            booking.Email,
                            booking.Phone,
                            service.Name,
                            booking.Date.ToLocalTime()
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("EMAIL BACKGROUND ERROR: " + ex.Message);
                    }
                });

                Console.WriteLine("EMAIL SENT");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EMAIL ERROR: " + ex.ToString());
                throw;
            }

            return Ok(booking);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var bookings = _context.Bookings
                .Include(b => b.Service)
                .OrderByDescending(b => b.Date)
                .ToList();

            return Ok(bookings);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null)
                return NotFound();

            _context.Bookings.Remove(booking);
            _context.SaveChanges();
            return Ok();
        }
        [HttpGet("available")]
        [AllowAnonymous]
        public IActionResult GetAvailable([FromQuery] string date, [FromQuery] int serviceId)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date");

            var service = _context.Services.Find(serviceId);
            if (service == null)
                return BadRequest("Service not found");

            var startLocal = parsedDate.Date;
            var endLocal = startLocal.AddDays(1);

            var startUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();
            var endUtc = DateTime.SpecifyKind(endLocal, DateTimeKind.Local).ToUniversalTime();

            var bookings = _context.Bookings
                .Where(b => b.ServiceId == serviceId &&
                            b.Date >= startUtc && b.Date < endUtc)
                .ToList();

            var bookedSlots = bookings
                .Select(b => b.Date.ToLocalTime().TimeOfDay)
                .ToList();


            var available = allowedSlots
                .Where(slot => !bookedSlots.Contains(slot))
                .Select(t => t.ToString(@"hh\:mm"))
                .ToList();

            return Ok(available);
        }
        [HttpGet("month")]
        [AllowAnonymous]
        public IActionResult GetMonth(int year, int month, int serviceId)
        {
            var service = _context.Services.Find(serviceId);
            if (service == null)
                return BadRequest("Service not found");

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var result = new List<object>();

            for (int day = 1; day <= daysInMonth; day++)
            {
                var localDate = DateTime.SpecifyKind(
                    new DateTime(year, month, day),
                    DateTimeKind.Local
                );

                var utcDate = localDate.ToUniversalTime();

                var start = utcDate.Date;   
                var end = start.AddDays(1);

                var bookingsCount = _context.Bookings
                    .Where(b => b.ServiceId == serviceId &&
                                b.Date >= start && b.Date < end)
                    .Count(); 

                result.Add(new
                {
                    date = localDate.ToString("yyyy-MM-dd"),
                    available = bookingsCount < allowedSlots.Count
                });
            }

            return Ok(result);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

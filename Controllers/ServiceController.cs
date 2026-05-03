using Microsoft.AspNetCore.Mvc;
using MakeupBookingAPI.Data;
using MakeupBookingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace MakeupBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ServiceController(AppDbContext context)
        {
            _context = context;
        }
        // ✅ Отримати всі послуги
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll()
        {
            var services = _context.Services.ToList();
            return Ok(services);
        }
        // 👑 Додавання нової послуги (для адміністратора)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] Service service)
        {
            if (service == null || string.IsNullOrWhiteSpace(service.Name))
                return BadRequest("Невірні дані");
            _context.Services.Add(service);
            _context.SaveChanges();
            return Ok(service);
        }
        // 👑 Видалення послуги
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var service = _context.Services.Find(id);
            if (service == null)
                return NotFound();
            _context.Services.Remove(service);
            _context.SaveChanges();
            return Ok();
        }
    }
}
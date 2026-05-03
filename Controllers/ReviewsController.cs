using System;
using Microsoft.AspNetCore.Mvc;
using MakeupBookingAPI.Data;
using MakeupBookingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace MakeupBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll()
        {
            return Ok(_context.Reviews.OrderByDescending(r => r.CreatedAt).ToList());
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Create([FromBody] Review review)
        {
            if (review == null || string.IsNullOrWhiteSpace(review.Name) || string.IsNullOrWhiteSpace(review.Text))
                return BadRequest("¬вед≥ть ус≥ пол€");

            review.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(review);
            _context.SaveChanges();

            return Ok(review);
        }
    }
}

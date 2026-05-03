using System;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MakeupBookingAPI.Data;
using MakeupBookingAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
namespace MakeupBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return Conflict("Email вже зареєстрований");
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("Реєстрація успішна");
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Невірний логін або пароль");
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyString = _config["Jwt:Key"];

            if (string.IsNullOrEmpty(keyString))
                throw new Exception("JWT key is missing");

            var key = Encoding.UTF8.GetBytes(keyString);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }
    }
}
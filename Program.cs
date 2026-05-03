using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MakeupBookingAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MakeupBookingAPI.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE =====
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// ===== CONTROLLERS =====
builder.Services.AddControllers();

// ===== SWAGGER =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== EMAIL SERVICE =====
builder.Services.AddScoped<EmailService>();

// ===== JWT =====
var jwtKey = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// ===== CORS (ОСЬ ГОЛОВНЕ ВИПРАВЛЕННЯ) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ===== AUTO MIGRATION + SEED =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.Migrate();

    // якщо сервісів ще нема — додаємо
    if (!db.Services.Any())
    {
        db.Services.AddRange(
            new Service { Name = "Makeup", Price = 150, DurationMinutes = 60 },
            new Service { Name = "Hair", Price = 200, DurationMinutes = 90 },
            new Service { Name = "Brows", Price = 80, DurationMinutes = 45 }
        );

        db.SaveChanges();
    }
}

// ===== MIDDLEWARE =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ❗ ВАЖЛИВО: CORS ДО AUTH
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
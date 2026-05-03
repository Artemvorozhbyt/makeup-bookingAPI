using MakeupBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MakeupBookingAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Booking ↔ Service
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany()
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Review constraints
            modelBuilder.Entity<Review>()
                .Property(r => r.Rating)
                .HasDefaultValue(5)
                .IsRequired();

            modelBuilder.Entity<Review>()
                .ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5"));


            // 🔹 User role by default
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue("User");
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.Date);
        }
    }
}

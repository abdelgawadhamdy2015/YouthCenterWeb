using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.Data
{
    public class AppDbContext : DbContext
    {


        public DbSet<User> Users { get; set; }
        public DbSet<YouthCenter> YouthCenters { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<Role> Roles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configure Reservation -> Activity
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Activity)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.ActivityId)
                .OnDelete(DeleteBehavior.Restrict); // Change to Restrict

            // 2. Configure Reservation -> User
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Change to Restrict

            // 3. Configure Reservation -> YouthCenter
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.YouthCenter)
                .WithMany(y => y.Reservations)
                .HasForeignKey(r => r.YouthCenterId)
                .OnDelete(DeleteBehavior.Restrict); // Change to Restrict

            // 4. Configure User -> YouthCenter
            modelBuilder.Entity<User>()
                .HasOne(u => u.YouthCenter)
                .WithMany(y => y.Users)
                .HasForeignKey(u => u.YouthCenterId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<YouthCenter>().HasData(new YouthCenter { Id = 1, Name = "c1", Mobile = "1" }, new YouthCenter { Id = 2, Name = "c2", Mobile = "2" }, new YouthCenter { Id = 3, Name = "c3", Mobile = "3" });

            modelBuilder.Entity<Role>().HasData(new Role { Id = 1, Name = "User" }, new Role { Id = 2, Name = "Admin" }, new Role { Id = 3, Name = "SuperAdmin" });
        }
    }
}
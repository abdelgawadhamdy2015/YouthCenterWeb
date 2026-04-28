using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.Data
{
    public class AppDbContext : DbContext
    {


        public DbSet<User> Users { get; set; }
        public DbSet<YouthCenter> YouthCenters { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<YouthCenterActivity> YouthCenterActivities { get; set; }

        public DbSet<Activity> Activities { get; set; }
        public DbSet<Role> Roles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Configure User -> YouthCenter
            modelBuilder.Entity<User>()
                .HasOne(u => u.YouthCenter).WithMany()
                .HasForeignKey(u => u.YouthCenterId)
                .OnDelete(DeleteBehavior.Restrict);
            // 2. Configure Reservation -> User
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Change to Restrict

            // 3. Configure Reservation -> YouthCenterActivity
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.YouthCenterActivity)
                .WithMany(y => y.Reservations)
                .HasForeignKey(r => r.YouthCenterActivityId)
                .OnDelete(DeleteBehavior.Restrict); // Change to Restrict



            // 4. Configure YouthCenterActivity -> YouthCenter
            modelBuilder.Entity<YouthCenterActivity>()
                .HasOne(yca => yca.YouthCenter)
                .WithMany(y => y.YouthCenterActivities)
                .HasForeignKey(yca => yca.YouthCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Configure YouthCenterActivity -> Activity
            modelBuilder.Entity<YouthCenterActivity>()
                .HasOne(yca => yca.Activity)
                .WithMany()
                .HasForeignKey(yca => yca.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .Property(x => x.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<YouthCenterActivity>()
                        .Property(x => x.Price)
                        .HasPrecision(18, 2);
        }
    }
}
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<YouthCenter> YouthCenters { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<YouthCenterActivity> YouthCenterActivities { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Cup> Cups { get; set; }
    public DbSet<CupGroup> CupGroups { get; set; }
    public DbSet<CupTeam> CupTeams { get; set; }
    public DbSet<CupMatch> CupMatches { get; set; }
    public DbSet<OtpEntry> OtpEntries { get; set; }
    public DbSet<DeviceToken> DeviceTokens { get; set; }
    public DbSet<UserNotification> Notifications { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
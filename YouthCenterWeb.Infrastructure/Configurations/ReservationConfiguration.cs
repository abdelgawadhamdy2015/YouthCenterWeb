using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Configuration;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(r => r.Date)
            .IsRequired();

        builder.Property(r => r.StartTime)
            .IsRequired();

        builder.Property(r => r.EndTime)
            .IsRequired();

        builder.Property(r => r.Status)
      .IsRequired()
      .HasConversion<string>()
      .HasMaxLength(50);

        builder.Property(r => r.SpecialRequest)
            .HasMaxLength(500);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.YouthCenterActivity)
            .WithMany(yca => yca.Reservations)
            .HasForeignKey(r => r.YouthCenterActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.YouthCenterActivityId, r.Date, r.StartTime, r.EndTime })
     .HasFilter("[Status] IN ('AwaitingPayment', 'Confirmed')")
     .IsUnique();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Configuration;

public class YouthCenterActivityConfiguration : IEntityTypeConfiguration<YouthCenterActivity>
{
    public void Configure(EntityTypeBuilder<YouthCenterActivity> builder)
    {
        builder.ToTable("YouthCenterActivities");

        builder.HasKey(yca => yca.Id);

        builder.Property(yca => yca.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(yca => yca.MaxCapacity)
            .IsRequired();

        builder.Property(yca => yca.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(yca => yca.YouthCenter)
            .WithMany(y => y.YouthCenterActivities)
            .HasForeignKey(yca => yca.YouthCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(yca => yca.Activity)
            .WithMany()
            .HasForeignKey(yca => yca.ActivityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(yca => yca.Reservations)
            .WithOne(r => r.YouthCenterActivity)
            .HasForeignKey(r => r.YouthCenterActivityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
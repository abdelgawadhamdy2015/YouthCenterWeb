using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Configuration;

public class YouthCenterConfiguration : IEntityTypeConfiguration<YouthCenter>
{
    public void Configure(EntityTypeBuilder<YouthCenter> builder)
    {
        builder.ToTable("YouthCenters");

        builder.HasKey(y => y.Id);

        builder.Property(y => y.NameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(y => y.Phone)
            .HasMaxLength(20);

        builder.Property(y => y.Email)
            .HasMaxLength(200);

        builder.Property(y => y.Address)
            .HasMaxLength(500);

        builder.Property(y => y.Description)
            .HasMaxLength(1000);

        builder.Property(y => y.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(y => y.YouthCenterActivities)
            .WithOne(yca => yca.YouthCenter)
            .HasForeignKey(yca => yca.YouthCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
// Infrastructure/Configuration/DeviceTokenConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Configuration;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("DeviceTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(300); // FCM tokens run ~150-200 chars; 300 gives headroom

        builder.Property(t => t.Platform)
            .IsRequired()
            .HasMaxLength(20); // "android" / "ios" / "web"

        builder.Property(t => t.UpdatedAt)
            .IsRequired();

        // ✅ One device-token row per user — enforces the "single active
        // token per account" rule at the DB level, not just in application code.
        builder.HasIndex(t => t.UserId)
            .IsUnique();

        // ✅ A given physical token also shouldn't map to two different users
        // at once (covers the "shared device, new user logs in" case from
        // AddOrUpdateAsync before the app-level cleanup runs).
        builder.HasIndex(t => t.Token)
            .IsUnique();

        builder.HasOne<User>()
      .WithMany()
      .HasForeignKey(t => t.UserId)
      .OnDelete(DeleteBehavior.Cascade);// deleting a user cleans up their token row
    }
}
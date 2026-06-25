using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Configurations
{
    // ─── Cup ─────────────────────────────────────────────────────────────────

    public class CupConfiguration : IEntityTypeConfiguration<Cup>
    {
        public void Configure(EntityTypeBuilder<Cup> builder)
        {
            builder.ToTable("Cups");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.NameAr)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.Description)
                .HasMaxLength(1000);

            builder.Property(c => c.Format)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(c => c.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(c => c.MaxTeams).IsRequired();

            builder.Property(c => c.GroupSize)
                .IsRequired()
                .HasDefaultValue(4);

            builder.Property(c => c.TeamsAdvancingPerGroup)
                .IsRequired()
                .HasDefaultValue(2);

            builder.Property(c => c.CurrentTeamCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.EntryFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(c => c.Activity)
                .WithMany()
                .HasForeignKey(c => c.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.YouthCenter)
                .WithMany()
                .HasForeignKey(c => c.YouthCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.CreatedByAdmin)
                .WithMany()
                .HasForeignKey(c => c.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Teams)
                .WithOne(t => t.Cup)
                .HasForeignKey(t => t.CupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Groups)
                .WithOne(g => g.Cup)
                .HasForeignKey(g => g.CupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Matches)
                .WithOne(m => m.Cup)
                .HasForeignKey(m => m.CupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // ─── CupGroup ─────────────────────────────────────────────────────────────

    public class CupGroupConfiguration : IEntityTypeConfiguration<CupGroup>
    {
        public void Configure(EntityTypeBuilder<CupGroup> builder)
        {
            builder.ToTable("CupGroups");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(20);    // "Group A", "Group B" etc.

            builder.Property(g => g.GroupNumber).IsRequired();

            builder.Property(g => g.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique: one group number per cup
            builder.HasIndex(g => new { g.CupId, g.GroupNumber }).IsUnique();

            builder.HasMany(g => g.Teams)
                .WithOne(t => t.Group)
                .HasForeignKey(t => t.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(g => g.Matches)
                .WithOne(m => m.Group)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // ─── CupTeam ──────────────────────────────────────────────────────────────

    public class CupTeamConfiguration : IEntityTypeConfiguration<CupTeam>
    {
        public void Configure(EntityTypeBuilder<CupTeam> builder)
        {
            builder.ToTable("CupTeams");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TeamName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(t => t.RegisteredAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Standings columns — updated by service after each match result
            builder.Property(t => t.Points).HasDefaultValue(0);
            builder.Property(t => t.Wins).HasDefaultValue(0);
            builder.Property(t => t.Draws).HasDefaultValue(0);
            builder.Property(t => t.Losses).HasDefaultValue(0);
            builder.Property(t => t.GoalsFor).HasDefaultValue(0);
            builder.Property(t => t.GoalsAgainst).HasDefaultValue(0);

            // Prevent duplicate requests: one active registration per cup per captain
            builder.HasIndex(t => new { t.CupId, t.CaptainUserId }).IsUnique();

            builder.HasOne(t => t.CaptainUser)
                .WithMany()
                .HasForeignKey(t => t.CaptainUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Group relationship is configured in CupGroupConfiguration
            // to avoid duplicate HasMany/WithOne conflicts

            builder.HasMany(t => t.MatchesAsTeamA)
                .WithOne(m => m.TeamA)
                .HasForeignKey(m => m.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.MatchesAsTeamB)
                .WithOne(m => m.TeamB)
                .HasForeignKey(m => m.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.MatchesWon)
                .WithOne(m => m.Winner)
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // ─── CupMatch ─────────────────────────────────────────────────────────────

    public class CupMatchConfiguration : IEntityTypeConfiguration<CupMatch>
    {
        public void Configure(EntityTypeBuilder<CupMatch> builder)
        {
            builder.ToTable("CupMatches");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Stage)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(m => m.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(m => m.Round).IsRequired();

            builder.Property(m => m.MatchNumber).IsRequired();

            builder.Property(m => m.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique slot per cup/round/match position
            builder.HasIndex(m => new { m.CupId, m.Round, m.MatchNumber, m.Stage })
                .IsUnique();

            builder.HasOne(m => m.TeamA)
                .WithMany(t => t.MatchesAsTeamA)
                .HasForeignKey(m => m.TeamAId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.TeamB)
                .WithMany(t => t.MatchesAsTeamB)
                .HasForeignKey(m => m.TeamBId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Winner)
                .WithMany(t => t.MatchesWon)
                .HasForeignKey(m => m.WinnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Group is configured in CupGroupConfiguration
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Configuration
{
    // ─── Cup ─────────────────────────────────────────────────────────────────

    public class CupConfiguration : IEntityTypeConfiguration<Cup>
    {
        public void Configure(EntityTypeBuilder<Cup> builder)
        {
            builder.ToTable("Cups");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
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

            builder.Property(c => c.MaxTeams)
                .IsRequired();

            builder.Property(c => c.CurrentTeamCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.EntryFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // ── Relationships ────────────────────────────────────────────────

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

            builder.HasMany(c => c.Matches)
                .WithOne(m => m.Cup)
                .HasForeignKey(m => m.CupId)
                .OnDelete(DeleteBehavior.Cascade);
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

            // One team per cup per captain — prevent duplicate registrations
            builder.HasIndex(t => new { t.CupId, t.CaptainUserId })
                .IsUnique();

            // ── Relationships ────────────────────────────────────────────────

            builder.HasOne(t => t.CaptainUser)
                .WithMany()
                .HasForeignKey(t => t.CaptainUserId)
                .OnDelete(DeleteBehavior.Restrict);

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

            builder.Property(m => m.Round)
                .IsRequired();

            builder.Property(m => m.MatchNumber)
                .IsRequired();

            builder.Property(m => m.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(m => m.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint: one slot per round/match position per cup
            builder.HasIndex(m => new { m.CupId, m.Round, m.MatchNumber })
                .IsUnique();

            // ── Relationships (all nullable — bracket slots fill over time) ──

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
        }
    }
}
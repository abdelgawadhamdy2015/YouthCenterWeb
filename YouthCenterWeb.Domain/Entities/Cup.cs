using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    public class Cup
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // ── Foreign keys ────────────────────────────────────────────────────

        public int ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;

        public int YouthCenterId { get; set; }
        public YouthCenter YouthCenter { get; set; } = null!;

        public int CreatedByAdminId { get; set; }
        public User CreatedByAdmin { get; set; } = null!;

        // ── Cup settings ────────────────────────────────────────────────────

        public CupFormat Format { get; set; } = CupFormat.SingleElimination;

        public CupStatus Status { get; set; } = CupStatus.Draft;

        /// <summary>
        /// Must be a power of 2 (8, 16, 32) for elimination brackets.
        /// Enforced at the service layer.
        /// </summary>
        public int MaxTeams { get; set; }

        /// <summary>
        /// Auto-incremented when a CupTeam request is accepted.
        /// When it reaches MaxTeams, matches are auto-generated.
        /// </summary>
        public int CurrentTeamCount { get; set; } = 0;

        public double? EntryFee { get; set; }

        public DateTime RegistrationDeadline { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ──────────────────────────────────────────────────────

        public ICollection<CupTeam> Teams { get; set; } = new List<CupTeam>();

        public ICollection<CupMatch> Matches { get; set; } = new List<CupMatch>();
    }
}
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    public class Cup
    {
        public int Id { get; set; }

        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;

        public string? Description { get; set; }

        // ── Foreign keys ────────────────────────────────────────────────────

        public int ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;

        public int YouthCenterId { get; set; }
        public YouthCenter YouthCenter { get; set; } = null!;

        public int CreatedByAdminId { get; set; }
        public User CreatedByAdmin { get; set; } = null!;

        // ── Cup settings ────────────────────────────────────────────────────

        public CupFormat Format { get; set; } = CupFormat.RoundRobin;

        public CupStatus Status { get; set; } = CupStatus.Draft;

        /// <summary>
        /// Total teams in the cup. Must be divisible by GroupSize.
        /// e.g. MaxTeams=8, GroupSize=4 → 2 groups of 4.
        /// </summary>
        public int MaxTeams { get; set; }

        /// <summary>
        /// How many teams per group. Default = 4.
        /// Validated by service: MaxTeams % GroupSize == 0.
        /// </summary>
        public int GroupSize { get; set; } = 4;

        /// <summary>
        /// How many teams from each group advance to knockout.
        /// Default = 2 (top 2 advance).
        /// </summary>
        public int TeamsAdvancingPerGroup { get; set; } = 2;

        /// <summary>Auto-incremented when a CupTeam is accepted.</summary>
        public int CurrentTeamCount { get; set; } = 0;

        public double? EntryFee { get; set; }

        public DateTime RegistrationDeadline { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ──────────────────────────────────────────────────────

        public ICollection<CupTeam> Teams { get; set; } = new List<CupTeam>();
        public ICollection<CupGroup> Groups { get; set; } = new List<CupGroup>();
        public ICollection<CupMatch> Matches { get; set; } = new List<CupMatch>();
    }
}
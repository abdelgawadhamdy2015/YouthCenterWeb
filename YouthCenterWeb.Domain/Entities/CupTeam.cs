using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    /// <summary>
    /// A team's registration for a cup.
    /// Status flows: Pending → Accepted | Rejected
    /// GroupId is null until the random group draw happens
    /// (triggered when CurrentTeamCount == MaxTeams).
    /// </summary>
    public class CupTeam
    {
        public int Id { get; set; }

        // ── Foreign keys ────────────────────────────────────────────────────

        public int CupId { get; set; }
        public Cup Cup { get; set; } = null!;

        public int CaptainUserId { get; set; }
        public User CaptainUser { get; set; } = null!;

        /// <summary>
        /// Assigned after the random draw when the cup fills.
        /// Null while status is Pending or Rejected.
        /// </summary>
        public int? GroupId { get; set; }
        public CupGroup? Group { get; set; }

        // ── Team info ───────────────────────────────────────────────────────

        public string TeamName { get; set; } = string.Empty;

        public CupTeamStatus Status { get; set; } = CupTeamStatus.Pending;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        /// <summary>Null until an admin accepts or rejects.</summary>
        public DateTime? ReviewedAt { get; set; }

        // ── Standings (computed from group matches, cached here for perf) ──

        public int Points { get; set; } = 0;    // Win=3, Draw=1, Loss=0
        public int Wins { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int GoalsFor { get; set; } = 0;
        public int GoalsAgainst { get; set; } = 0;

        // ── Navigation ──────────────────────────────────────────────────────

        public ICollection<CupMatch> MatchesAsTeamA { get; set; } = new List<CupMatch>();
        public ICollection<CupMatch> MatchesAsTeamB { get; set; } = new List<CupMatch>();
        public ICollection<CupMatch> MatchesWon { get; set; } = new List<CupMatch>();
    }
}
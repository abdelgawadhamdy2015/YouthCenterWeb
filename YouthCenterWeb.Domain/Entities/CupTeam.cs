using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    /// <summary>
    /// Represents a team's registration request for a cup.
    /// Created when a user submits a join request.
    /// Status is Pending until an admin accepts or rejects.
    /// </summary>
    public class CupTeam
    {
        public int Id { get; set; }

        // ── Foreign keys ────────────────────────────────────────────────────

        public int CupId { get; set; }
        public Cup Cup { get; set; } = null!;

        /// <summary>
        /// The user who submitted the join request and leads the team.
        /// </summary>
        public int CaptainUserId { get; set; }
        public User CaptainUser { get; set; } = null!;

        // ── Team info ───────────────────────────────────────────────────────

        public string TeamName { get; set; } = string.Empty;

        public CupTeamStatus Status { get; set; } = CupTeamStatus.Pending;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Set when an admin accepts or rejects. Null = not yet reviewed.
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────

        /// <summary>Matches where this team plays as Team A.</summary>
        public ICollection<CupMatch> MatchesAsTeamA { get; set; } = new List<CupMatch>();

        /// <summary>Matches where this team plays as Team B.</summary>
        public ICollection<CupMatch> MatchesAsTeamB { get; set; } = new List<CupMatch>();

        /// <summary>Matches this team won.</summary>
        public ICollection<CupMatch> MatchesWon { get; set; } = new List<CupMatch>();
    }
}
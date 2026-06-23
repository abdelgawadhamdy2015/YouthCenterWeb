using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    /// <summary>
    /// A single match inside a cup bracket.
    ///
    /// All rounds are generated upfront when the cup fills.
    /// Later rounds start with TeamAId / TeamBId = null —
    /// they are filled automatically when earlier-round winners advance.
    ///
    /// Round numbering:
    ///   1 = first round (e.g. quarter-finals in an 8-team cup)
    ///   2 = semi-finals
    ///   3 = final
    ///
    /// MatchNumber is 1-based position within the round.
    /// Winners of MatchNumber N advance to ceil(N/2) in the next round.
    /// </summary>
    public class CupMatch
    {
        public int Id { get; set; }

        // ── Foreign keys ────────────────────────────────────────────────────

        public int CupId { get; set; }
        public Cup Cup { get; set; } = null!;

        /// <summary>Null until the previous round determines the team.</summary>
        public int? TeamAId { get; set; }
        public CupTeam? TeamA { get; set; }

        /// <summary>Null until the previous round determines the team.</summary>
        public int? TeamBId { get; set; }
        public CupTeam? TeamB { get; set; }

        /// <summary>Null until the match is completed.</summary>
        public int? WinnerId { get; set; }
        public CupTeam? Winner { get; set; }

        // ── Match info ──────────────────────────────────────────────────────

        public int Round { get; set; }

        public int MatchNumber { get; set; }

        public int? ScoreA { get; set; }

        public int? ScoreB { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public CupMatchStatus Status { get; set; } = CupMatchStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
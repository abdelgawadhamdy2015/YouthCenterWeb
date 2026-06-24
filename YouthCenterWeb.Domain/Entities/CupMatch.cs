
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    /// <summary>
    /// A single match inside a cup.
    ///
    /// Group stage matches  → GroupId is set, Stage = GroupStage
    /// Knockout matches     → GroupId is null, Stage = SemiFinal | Final etc.
    ///
    /// Knockout slots are created with TeamAId/TeamBId = null.
    /// They are filled automatically when group standings are finalised
    /// or when a previous knockout match completes.
    ///
    /// Round + MatchNumber are used only for knockout bracket positioning:
    ///   Round 1 = first knockout round (QF / SF depending on group count)
    ///   Winner of MatchNumber N → advances to ceil(N/2) in Round+1
    /// </summary>
    public class CupMatch
    {
        public int Id { get; set; }

        // ── Foreign keys ────────────────────────────────────────────────────

        public int CupId { get; set; }
        public Cup Cup { get; set; } = null!;

        /// <summary>Set for GroupStage matches. Null for knockout matches.</summary>
        public int? GroupId { get; set; }
        public CupGroup? Group { get; set; }

        public int? TeamAId { get; set; }
        public CupTeam? TeamA { get; set; }

        public int? TeamBId { get; set; }
        public CupTeam? TeamB { get; set; }

        public int? WinnerId { get; set; }
        public CupTeam? Winner { get; set; }

        // ── Match info ──────────────────────────────────────────────────────

        public CupMatchStage Stage { get; set; } = CupMatchStage.GroupStage;

        public CupMatchStatus Status { get; set; } = CupMatchStatus.Pending;

        /// <summary>
        /// Used for knockout positioning only.
        /// For group matches Round = group match index (1, 2, 3…).
        /// </summary>
        public int Round { get; set; }

        public int MatchNumber { get; set; }

        public int? ScoreA { get; set; }

        public int? ScoreB { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
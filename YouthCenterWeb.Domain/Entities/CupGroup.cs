namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    /// <summary>
    /// A group in the group stage of a Round Robin cup.
    /// Created automatically when the cup fills and teams are drawn randomly.
    ///
    /// Example: 8-team cup → Group A (teams 1–4), Group B (teams 5–8)
    /// Each group generates n*(n-1)/2 round-robin matches internally.
    /// Top N teams from each group advance to the knockout stage.
    /// </summary>
    public class CupGroup
    {
        public int Id { get; set; }

        public int CupId { get; set; }
        public Cup Cup { get; set; } = null!;

        /// <summary>e.g. "Group A", "Group B"</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>1-based position used for cross-seeding (A1 vs B2 etc.)</summary>
        public int GroupNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ──────────────────────────────────────────────────────

        /// <summary>Teams assigned to this group after the random draw.</summary>
        public ICollection<CupTeam> Teams { get; set; } = new List<CupTeam>();

        /// <summary>All round-robin matches inside this group.</summary>
        public ICollection<CupMatch> Matches { get; set; } = new List<CupMatch>();
    }
}
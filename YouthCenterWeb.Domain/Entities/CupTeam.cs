using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{

    public class CupTeam
    {
        public int Id { get; set; }


        public int CupId { get; set; }
        public Cup Cup { get; set; } = null!;

        public int? CaptainUserId { get; set; }
        public User CaptainUser { get; set; } = null!;


        public int? GroupId { get; set; }
        public CupGroup? Group { get; set; }


        public string TeamName { get; set; } = string.Empty;

        public CupTeamStatus Status { get; set; } = CupTeamStatus.Pending;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }


        public int Points { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int GoalsFor { get; set; } = 0;
        public int GoalsAgainst { get; set; } = 0;
        public int FairPlayPoints { get; set; }
        // ── Navigation ──────────────────────────────────────────────────────

        public ICollection<CupMatch> MatchesAsTeamA { get; set; } = new List<CupMatch>();
        public ICollection<CupMatch> MatchesAsTeamB { get; set; } = new List<CupMatch>();
        public ICollection<CupMatch> MatchesWon { get; set; } = new List<CupMatch>();
    }
}
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CupTeamDto
    {
        public int Id { get; set; }

        public int CupId { get; set; }

        public int CaptainUserId { get; set; }

        public string? CaptainName { get; set; }

        public int? GroupId { get; set; }

        public string? GroupName { get; set; }

        public string TeamName { get; set; } = string.Empty;

        public CupTeamStatus Status { get; set; }

        public DateTime RegisteredAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public int Points { get; set; }

        public int Wins { get; set; }

        public int Draws { get; set; }

        public int Losses { get; set; }

        public int GoalsFor { get; set; }

        public int GoalsAgainst { get; set; }

        public int GoalDifference { get; set; }

        public int PlayedMatches { get; set; }
    }
}
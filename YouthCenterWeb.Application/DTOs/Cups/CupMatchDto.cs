using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CupMatchDto
    {
        public int Id { get; set; }

        public int CupId { get; set; }

        public int? GroupId { get; set; }

        public string? GroupName { get; set; }

        public int? TeamAId { get; set; }

        public string? TeamAName { get; set; }

        public int? TeamBId { get; set; }

        public string? TeamBName { get; set; }

        public int? WinnerId { get; set; }

        public string? WinnerName { get; set; }

        public CupMatchStage Stage { get; set; }

        public CupMatchStatus Status { get; set; }

        public int Round { get; set; }

        public int MatchNumber { get; set; }

        public int? ScoreA { get; set; }

        public int? ScoreB { get; set; }

        public DateTime? ScheduledAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
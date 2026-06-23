using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CupDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int ActivityId { get; set; }
        public string ActivityName { get; set; } = string.Empty;

        public int YouthCenterId { get; set; }
        public string YouthCenterName { get; set; } = string.Empty;

        public int CreatedByAdminId { get; set; }
        public string CreatedByAdminName { get; set; } = string.Empty;

        public CupFormat Format { get; set; }
        public CupStatus Status { get; set; }

        public int MaxTeams { get; set; }
        public int CurrentTeamCount { get; set; }

        public double? EntryFee { get; set; }

        public DateTime RegistrationDeadline { get; set; }
        public DateTime StartDate { get; set; }

        public DateTime CreatedAt { get; set; }

        // Optional: lightweight info
        public int TeamsCount { get; set; }
        public int MatchesCount { get; set; }
    }
}
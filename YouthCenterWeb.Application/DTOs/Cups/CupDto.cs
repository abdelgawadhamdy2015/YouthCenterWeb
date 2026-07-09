using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CupDto
    {
        public int Id { get; set; }

        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int YouthCenterActivityId { get; set; }

        public string? ActivityNameAr { get; set; }
        public string? ActivityNameEn { get; set; }

        public string? YouthCenterNameAr { get; set; }
        public string? YouthCenterNameEn { get; set; }
        public double? YouthCenterLatitude { get; set; }
        public double? YouthCenterLongitude { get; set; }

        public int CreatedByAdminId { get; set; }
        public string? CreatedByAdminName { get; set; }

        public CupFormat Format { get; set; }

        public CupStatus Status { get; set; }

        public int MaxTeams { get; set; }
        public int GroupSize { get; set; }
        public int TeamsAdvancingPerGroup { get; set; }
        public int CurrentTeamCount { get; set; }

        public double? EntryFee { get; set; }

        public DateTime RegistrationDeadline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
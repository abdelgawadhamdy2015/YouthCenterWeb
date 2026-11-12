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
        public CupScheduleMode ScheduleMode { get; set; } = CupScheduleMode.Automatic;
        public TimeOnly? DailyStartTime { get; set; }
        public TimeOnly? DailyEndTime { get; set; }
        public int MatchDurationMinutes { get; set; } = 45;
        public int BreakMinutes { get; set; } = 15;
        public int HalftimeMinutes { get; set; } = 15;
        public int BreakBetweenMatchesMinutes { get; set; } = 0;
        public int PlayDaysMask { get; set; } = 127;

        public int RestDaysBetweenRounds { get; set; } = 0;
        public DateTime RegistrationDeadline { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
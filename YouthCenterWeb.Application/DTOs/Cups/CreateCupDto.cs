using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CreateCupDto
    {
        public string NameAr { get; set; } = string.Empty;

        public string NameEn { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int YouthCenterActivityId { get; set; }

        public CupFormat Format { get; set; } = CupFormat.RoundRobin;
        public int MaxTeams { get; set; }

        public int GroupSize { get; set; } = 4;

        public int TeamsAdvancingPerGroup { get; set; } = 2;

        public double? EntryFee { get; set; }
        public CupScheduleMode ScheduleMode { get; set; } = CupScheduleMode.Automatic;
        public TimeOnly? DailyStartTime { get; set; }
        public TimeOnly? DailyEndTime { get; set; }
        public int MatchDurationMinutes { get; set; } = 45;
        public int BreakMinutes { get; set; } = 15;
        public DateTime RegistrationDeadline { get; set; }

        public DateTime StartDate { get; set; }
    }
}
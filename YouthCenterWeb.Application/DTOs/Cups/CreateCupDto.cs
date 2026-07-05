using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CreateCupDto
    {
        public string NameAr { get; set; } = string.Empty;

        public string NameEn { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int YouthCenterActivityId { get; set; }

        public CupFormat Format { get; set; }

        public int MaxTeams { get; set; }

        public int GroupSize { get; set; } = 4;

        public int TeamsAdvancingPerGroup { get; set; } = 2;

        public double? EntryFee { get; set; }

        public DateTime RegistrationDeadline { get; set; }

        public DateTime StartDate { get; set; }
    }
}
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class UpdateCupDto
    {
        public string? NameAr { get; set; } = string.Empty;

        public string? NameEn { get; set; } = string.Empty;

        public string? Description { get; set; }

        public CupFormat? Format { get; set; }

        public CupStatus? Status { get; set; }

        public int? MaxTeams { get; set; }

        public int? GroupSize { get; set; }

        public int? TeamsAdvancingPerGroup { get; set; }

        public double? EntryFee { get; set; }

        public DateTime? RegistrationDeadline { get; set; }

        public DateTime? StartDate { get; set; }
    }
}
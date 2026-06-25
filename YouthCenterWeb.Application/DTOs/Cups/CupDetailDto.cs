namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CupDetailDto : CupDto
    {
        public List<CupGroupDto> Groups { get; set; } = [];

        public List<CupTeamDto> Teams { get; set; } = [];

        public List<CupMatchDto> Matches { get; set; } = [];
    }
}
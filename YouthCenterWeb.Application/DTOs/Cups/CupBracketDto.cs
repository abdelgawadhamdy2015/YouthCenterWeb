namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CupBracketDto
    {
        public List<CupMatchDto> GroupMatches { get; set; } = [];

        public List<CupMatchDto> KnockoutMatches { get; set; } = [];
    }
}
namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CupGroupDto
    {
        public int Id { get; set; }

        public int CupId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int GroupNumber { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CupTeamDto> Teams { get; set; } = [];

        public List<CupMatchDto> Matches { get; set; } = [];
    }
}
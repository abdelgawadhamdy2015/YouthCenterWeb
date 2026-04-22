namespace YouthCenterWeb.Data.DTOs
{
    public class YouthCenterDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public string? Address { get; set; }

        public decimal? PricePerHour { get; set; }

        public string? Description { get; set; }
    }
}
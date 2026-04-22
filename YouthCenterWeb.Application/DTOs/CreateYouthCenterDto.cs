using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.Data.DTOs
{
    public class CreateYouthCenterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Mobile { get; set; } = string.Empty;

        public string? Address { get; set; }

        public decimal? PricePerHour { get; set; }

        public string? Description { get; set; }
    }
}
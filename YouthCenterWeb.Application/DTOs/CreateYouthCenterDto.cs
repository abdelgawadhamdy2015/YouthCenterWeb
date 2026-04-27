using System.ComponentModel.DataAnnotations;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.Data.DTOs
{
    public class CreateYouthCenterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Mobile { get; set; } = string.Empty;

        public string? Location { get; set; }

        public decimal? PricePerHour { get; set; }

        public string? Description { get; set; }
        public List<int>? ActivitiesIds { get; set; }
    }
}
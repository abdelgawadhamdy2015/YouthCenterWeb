using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class YouthCenterActivityDto
    {
        public int Id { get; set; }
        public string ActivityName { get; set; } = string.Empty;

        [Required]
        public int YouthCenterId { get; set; }

        [Required]
        public int ActivityId { get; set; }

        public bool? IsAvailable { get; set; } = true;

        public int? MaxCapacity { get; set; }

        public decimal? Price { get; set; } = 0;
    }
}
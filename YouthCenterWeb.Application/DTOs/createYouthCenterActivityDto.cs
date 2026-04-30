using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class CreateYouthCenterActivityDto
    {

        [Required]
        public int YouthCenterId { get; set; }
        [Required]

        public int ActivityId { get; set; }

        public bool? IsAvailable { get; set; } = true;

        public int? MaxCapacity { get; set; }

        public decimal? Price { get; set; } = 0;
    }
}
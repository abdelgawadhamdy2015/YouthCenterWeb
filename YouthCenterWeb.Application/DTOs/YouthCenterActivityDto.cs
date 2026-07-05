using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class YouthCenterActivityDto
    {
        public int Id { get; set; }
        public string ActivityNameAr { get; set; } = string.Empty;
        public string ActivityNameEn { get; set; } = string.Empty;
        public string YouthCenterNameAr { get; set; } = string.Empty;
        public string YouthCenterNameEn { get; set; } = string.Empty;
        [Required]
        public int YouthCenterId { get; set; }

        [Required]
        public int ActivityId { get; set; }

        public bool? IsAvailable { get; set; } = true;

        public int? MaxCapacity { get; set; }

        public decimal? Price { get; set; } = 0;
    }
}
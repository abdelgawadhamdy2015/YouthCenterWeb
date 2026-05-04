using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.Data.DTOs
{
    public class CreateActivityDto
    {
        [Required]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        public string NameEn { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;
    }
}
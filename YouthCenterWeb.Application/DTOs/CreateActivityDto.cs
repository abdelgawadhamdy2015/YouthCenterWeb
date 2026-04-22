using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.Data.DTOs
{
    public class CreateActivityDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int YouthCenterId { get; set; }
    }
}
// Data/DTOs/CreateAdminDto.cs
// used by SuperAdmin only — requires YouthCenterId
using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class CreateAdminDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Mobile { get; set; }

        [Required]
        public int? YouthCenterId { get; set; }
    }
}
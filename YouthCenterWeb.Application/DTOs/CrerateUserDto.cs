using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.Data.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Mobile { get; set; }

        public string? ImageUrl { get; set; }

        public int? YouthCenterId { get; set; }

        [Required]
        public int RoleId { get; set; } = 1;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
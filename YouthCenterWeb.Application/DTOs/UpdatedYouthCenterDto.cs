using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class UpdatedYouthCenterDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? City { get; set; }
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
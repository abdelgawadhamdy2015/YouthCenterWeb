using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class CreateYouthCenterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string? Address { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Description { get; set; }

        [JsonIgnore]
        public List<YouthCenterActivity>? YouthCenterActivities { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
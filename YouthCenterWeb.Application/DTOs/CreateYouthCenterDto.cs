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
        public string Mobile { get; set; } = string.Empty;

        public string? Location { get; set; }


        public string? Description { get; set; }
        [JsonIgnore]
        public List<YouthCenterActivity>? YouthCenterActivities { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
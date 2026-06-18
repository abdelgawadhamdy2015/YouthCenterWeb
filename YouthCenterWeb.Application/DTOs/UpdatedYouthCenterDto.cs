using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class UpdatedYouthCenterDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
        public string? Address { get; set; }

        public string? City { get; set; }
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
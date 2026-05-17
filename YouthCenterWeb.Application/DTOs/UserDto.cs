using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Bio { get; set; }
        public int? YouthCenterId { get; set; }
        public string? YouthCenterName { get; set; }
        public UserRole Role { get; set; }
        public int? FavActivityId { get; set; }
        public int ReservationCount { get; set; }
    }
}
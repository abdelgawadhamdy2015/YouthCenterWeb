// Data/DTOs/LoginData.cs

// Data/DTOs/LoginData.cs
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class LoginData
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string? YouthCenterName { get; set; }
        public int? FavActivityId { get; set; }
        public string? FavActivityName { get; set; }
        public int ReservationCount { get; set; } = 0;

        public UserRole Role { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
// Data/DTOs/LoginData.cs
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

public class LoginData
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Token { get; set; } = string.Empty;
}
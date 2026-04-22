namespace YouthCenterWeb.Data.DTOs;

public class LoginData
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string? Token { get; set; }
}
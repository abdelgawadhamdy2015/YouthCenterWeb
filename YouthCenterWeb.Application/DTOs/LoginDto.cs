namespace YouthCenterWeb.Data.DTO;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? Role { get; set; } = 1; // 1 for regular user, 2 for admin, 3 for super admin
}
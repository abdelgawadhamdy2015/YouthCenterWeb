using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.Data.DTO;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    public int? Role { get; set; } = 1; // 1 for regular user, 2 for admin, 3 for super admin
}
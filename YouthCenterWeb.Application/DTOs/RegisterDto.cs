// Data/DTOs/RegisterDto.cs
// used by public registration — no YouthCenterId, no Role
using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Mobile { get; set; }
}
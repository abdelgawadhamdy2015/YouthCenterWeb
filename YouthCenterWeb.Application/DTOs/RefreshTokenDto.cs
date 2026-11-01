using System.ComponentModel.DataAnnotations;
namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs;

public class RefreshTokenDto
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}
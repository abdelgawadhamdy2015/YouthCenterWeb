using System.ComponentModel.DataAnnotations;
namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs;

public class MarkRefundedDto
{
    [Required, MaxLength(100)]
    public string Reference { get; set; } = string.Empty;
}
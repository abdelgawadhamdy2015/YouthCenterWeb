using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class ResetPasswordDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        // بيتبعت من الـ App، بنقارنه تاني كطبقة أمان إضافية
        public string? Otp { get; set; }

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = default!;
    }
}
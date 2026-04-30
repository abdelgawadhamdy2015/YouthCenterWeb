// Core/DTOs/RejectReservationDto.cs
using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class RejectReasonDto
    {
        [Required]
        [MinLength(5)]
        public string Reason { get; set; } = string.Empty;
    }
}
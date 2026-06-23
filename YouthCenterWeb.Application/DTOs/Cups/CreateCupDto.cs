using System.ComponentModel.DataAnnotations;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class CreateCupDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required]
        public int YouthCenterId { get; set; }

        [Required]
        public int CreatedByAdminId { get; set; }

        [Required]
        public CupFormat Format { get; set; } = CupFormat.SingleElimination;

        [Required]
        public int MaxTeams { get; set; }

        public double? EntryFee { get; set; }

        [Required]
        public DateTime RegistrationDeadline { get; set; }

        [Required]
        public DateTime StartDate { get; set; }
    }
}
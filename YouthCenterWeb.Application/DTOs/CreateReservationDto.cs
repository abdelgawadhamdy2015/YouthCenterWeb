using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.Data.DTOs
{
    public class CreateReservationDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required]
        public int YouthCenterId { get; set; }
    }
}
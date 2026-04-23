using YouthCenterWeb.Data.DTOs;

namespace YouthCenterWeb.DTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public decimal? TotalPrice { get; set; } = 0;

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public int UserId { get; set; }
        public string? Username { get; set; } = string.Empty;
        public int ActivityId { get; set; }
        public string? ActivityName { get; set; } = string.Empty;

        public int YouthCenterId { get; set; }

        public string? YouthCenterName { get; set; } = string.Empty;

        public double DurationHours { get; set; }

    }
}
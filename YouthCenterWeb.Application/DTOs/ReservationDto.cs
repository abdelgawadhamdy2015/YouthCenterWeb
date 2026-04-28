using YouthCenterWeb.Data.DTOs;

namespace YouthCenterWeb.DTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public decimal? TotalPrice { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public int UserId { get; set; }
        public string? Username { get; set; } = string.Empty;
        public int YouthCenterActivityId { get; set; }
        public string? ActivityName { get; set; } = string.Empty;

        public string? YouthCenterName { get; set; } = string.Empty;

        public decimal DurationHours { get; set; }

    }
}
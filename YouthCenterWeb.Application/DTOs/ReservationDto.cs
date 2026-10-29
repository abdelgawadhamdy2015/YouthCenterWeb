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
        public string? PaymentUrl { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; } = string.Empty;
        public int YouthCenterActivityId { get; set; }
        public string? ActivityName { get; set; } = string.Empty;

        public int? ActivityId { get; set; }
        public int? YouthCenterId { get; set; }

        public string? YouthCenterName { get; set; } = string.Empty;
        public string? SpecialRequest { get; set; } = string.Empty;

        public decimal DurationHours { get; set; }
        public string? RejectionReason { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public DateTime? PaymentLockExpiresAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public DateTime? CancelledAt { get; set; }
        public bool RefundPending { get; set; }
        public string? PaymentTransactionId { get; set; }


    }
}
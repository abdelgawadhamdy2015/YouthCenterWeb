using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        public string? MerchantOrderId { get; set; }
        public string? PaymentTransactionId { get; set; }

        public decimal TotalPrice { get; set; }

        [Required]
        public ReservationStatus Status { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int YouthCenterActivityId { get; set; }

        [ForeignKey("YouthCenterActivityId")]
        public YouthCenterActivity? YouthCenterActivity { get; set; }

        public string? RejectionReason { get; set; }
        public string? SpecialRequest { get; set; } = string.Empty;

        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public DateTime? PaymentLockExpiresAt { get; set; }
        public DateTime? PaidAt { get; set; }

        // ── جديد ──
        public DateTime? CancelledAt { get; set; }
        public bool RefundPending { get; set; }

        [NotMapped]
        public decimal DurationHours
        {
            get
            {
                var duration = EndTime - StartTime;
                if (duration.TotalHours <= 0)
                    duration = duration.Add(TimeSpan.FromHours(24));
                return (decimal)duration.TotalHours;
            }
        }
    }
}
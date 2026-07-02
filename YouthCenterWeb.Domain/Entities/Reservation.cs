using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    public class Reservation
    {
        public int Id { get; set; }

        // 📅 تاريخ الحجز
        [Required]
        public DateTime Date { get; set; }

        // ⏰ وقت البداية
        [Required]
        public TimeOnly StartTime { get; set; }

        // ⏰ وقت النهاية
        [Required]
        public TimeOnly EndTime { get; set; }


        // 💰 السعر النهائي
        public decimal TotalPrice { set; get; }


        // 📌 حالة الحجز
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
        public string? PaymentTransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        // 🧠 مدة الحجز (مش متخزنة) 

        [NotMapped]
        public decimal DurationHours
        {
            get
            {
                var duration = EndTime - StartTime;

                // If EndTime is before or equal StartTime, it crosses midnight
                if (duration.TotalHours <= 0)
                    duration = duration.Add(TimeSpan.FromHours(24));

                return (decimal)duration.TotalHours;
            }
        }
    }
}
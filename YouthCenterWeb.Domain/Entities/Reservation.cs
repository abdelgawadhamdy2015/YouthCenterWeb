using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace YouthCenterWeb.Models
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
        public decimal? TotalPrice { get; set; } = 0;

        // 📌 حالة الحجز
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // 🔗 ===============================
        // 🔗 User
        // 🔗 ===============================
        public int UserId { get; set; }

        [JsonIgnore] // ❗ مهم لتجنب loop
        public User? User { get; set; }

        // 🔗 ===============================
        // 🔗 Activity
        // 🔗 ===============================
        public int ActivityId { get; set; }

        public Activity? Activity { get; set; }

        // 🔗 ===============================
        // 🔗 YouthCenter (اختياري لكن مفيد)
        // 🔗 ===============================
        public int YouthCenterId { get; set; }

        [JsonIgnore] // ❗ لتقليل حجم الـ JSON
        public YouthCenter? YouthCenter { get; set; }

        // 🧠 مدة الحجز (مش متخزنة)
        [NotMapped]
        public double DurationHours =>
            (EndTime - StartTime).TotalHours;
    }



}
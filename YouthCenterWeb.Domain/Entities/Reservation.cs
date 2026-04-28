using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

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
        public decimal TotalPrice { set; get; }


        // 📌 حالة الحجز
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;


        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }


        public int YouthCenterActivityId { get; set; }

        [ForeignKey("YouthCenterActivityId")]
        public YouthCenterActivity? YouthCenterActivity { get; set; }

        // 🧠 مدة الحجز (مش متخزنة)
        [NotMapped]
        public decimal DurationHours =>
           (decimal)(EndTime - StartTime).TotalHours
           ;

    }



}
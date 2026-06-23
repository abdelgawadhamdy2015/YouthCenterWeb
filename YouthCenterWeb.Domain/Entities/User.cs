using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public string? Bio { get; set; }


        // 🔗 Foreign Key → YouthCenter
        public int? YouthCenterId { get; set; } = null;

        // 🔗 Navigation
        public YouthCenter? YouthCenter { get; set; }

        // 🔗 Foreign Key → Activity
        public int? ActivityId { get; set; }

        // 🔗 Navigation
        public Activity? Activity { get; set; }


        // 🔗 Navigation
        public UserRole Role { get; set; } = UserRole.User;

        // 🔗 Reservations
        [JsonIgnore] //  لتقليل حجم الـ JSON
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // 🧠 محسوبة مش متخزنة
        [NotMapped]
        public int ReservationCount => Reservations?.Count ?? 0;

        // 🔐 Password (مش بيطلع في API)
        [JsonIgnore]
        public string HashedPassword { get; set; } = string.Empty;
    }
}
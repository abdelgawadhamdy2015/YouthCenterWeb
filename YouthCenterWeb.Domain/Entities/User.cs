using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Mobile { get; set; }

        public string? ImageUrl { get; set; }

        // 🔗 Foreign Key → YouthCenter
        public int? YouthCenterId { get; set; } = null;

        // 🔗 Navigation
        public YouthCenter? YouthCenter { get; set; }



        // 🔗 Navigation
        public UserRole Role { get; set; } = UserRole.User;

        // 🔗 Reservations
        [JsonIgnore] // ❗ لتقليل حجم الـ JSON
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // 🧠 محسوبة مش متخزنة
        [NotMapped]
        public int ReservationCount => Reservations?.Count ?? 0;

        // 🔐 Password (مش بيطلع في API)
        [JsonIgnore]
        public string HashedPassword { get; set; } = string.Empty;
    }
}
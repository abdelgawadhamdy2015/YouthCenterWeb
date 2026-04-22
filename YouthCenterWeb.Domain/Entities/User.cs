using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

        // 🔗 Foreign Key → Role
        public int RoleId { get; set; }

        // 🔗 Navigation
        public Role? Role { get; set; }

        // 🔗 Reservations
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // 🧠 محسوبة مش متخزنة
        [NotMapped]
        public int ReservationCount => Reservations?.Count ?? 0;

        // 🔐 Password (مش بيطلع في API)
        [JsonIgnore]
        public string HashedPassword { get; set; } = string.Empty;
    }
}
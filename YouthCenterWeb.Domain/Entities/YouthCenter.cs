using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace YouthCenterWeb.Models
{
    public class YouthCenter
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Mobile { get; set; } = string.Empty;

        public string? Address { get; set; }

        // 💰 استخدم decimal بدل double
        public decimal? PricePerHour { get; set; }

        public string? Description { get; set; }

        // 🔗 Activities
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();

        // 🔗 Users (مخفي عشان يمنع loop)
        [JsonIgnore]
        public ICollection<User> Users { get; set; } = new List<User>();

        // 🔗 Reservations
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
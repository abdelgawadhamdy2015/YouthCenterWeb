using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.Models
{
    public class YouthCenter
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Mobile { get; set; } = string.Empty;

        public string? Location { get; set; }

        // 💰 استخدم decimal بدل double
        public decimal? PricePerHour { get; set; }

        public string? Description { get; set; }

        // 🔗 Youth Center Activities

        public List<YouthCenterActivity> YouthCenterActivities { get; set; } = new();


        // 🔗 Users (مخفي عشان يمنع loop)
        [JsonIgnore]
        public List<User> Users { get; set; } = new List<User>();

        // 🔗 Reservations
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
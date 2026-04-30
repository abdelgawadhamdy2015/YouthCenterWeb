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

        public string? City { get; set; }

        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        public string? Address { get; set; }


        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }



        public List<YouthCenterActivity> YouthCenterActivities { get; set; } = new();




        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Models;
namespace YouthCenterWeb.Models
{
    [PrimaryKey("Id")]
    public class Activity
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!; // Football, PlayStation

        [JsonPropertyName("youthCenterId")]
        public int YouthCenterId { get; set; }

        [JsonPropertyName("youthCenter")]
        public YouthCenter YouthCenter { get; set; } = null!;


        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    }

}
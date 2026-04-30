using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    public class YouthCenterActivity
    {
        public int Id { get; set; }

        public int YouthCenterId { get; set; }
        public YouthCenter YouthCenter { get; set; } = null!;

        public int ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;

        public decimal? Price { get; set; } = 0;

        public int? MaxCapacity { get; set; }

        public bool? IsAvailable { get; set; } = true;

        // Navigation
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
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
    }
}
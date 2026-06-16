namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class AvailBookingDto
    {
        public DateTime Date { get; set; }

        public int YouthCenterId { get; set; }

        public int ActivityId { get; set; }

        public int? UserId { get; set; }

    }
}
namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class PeakHourDto
    {
        public int Hour { get; set; }
        public string HourLabel => $"{Hour:D2}:00";

        public int BookingCount { get; set; }
    }
}
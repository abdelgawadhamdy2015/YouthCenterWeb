namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class DashboardOverviewDto
    {
        public DashboardStatsDto? Stats { get; set; }

        public List<BookingStatusDto>? BookingStatuses { get; set; }

        public List<ActivityBreakdownDto>? ActivityBreakdown { get; set; }

        public List<PeakHourDto>? PeakBookingHours { get; set; }

        public int AllBookingCount { get; set; }

    }
}
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class DashboardService(
        IReservationRepo reservationRepo,
        IActivityService activityService) : IDashboardService
    {
        public async Task<DashboardOverviewDto> GetDashboardOverviewDtoAsync(int? youthCenterId, int? userId)
        {
            var filter = new FilteredReservationDto
            {
                YouthCenterId = youthCenterId,
                UserId = userId
            };

            var allBookingCount = await reservationRepo.GetCount(filter);
            var bookingStatuses = await GetBookingStatusesAsync(filter);
            var weeklyStats = await GetWeeklyStatsAsync(filter);
            var activityBreakdown = await GetActivityBreakdownAsync(filter, userId, youthCenterId);
            var peakHours = await reservationRepo.GetPeakBookingHoursAsync(filter);

            return new DashboardOverviewDto
            {
                AllBookingCount = allBookingCount,
                BookingStatuses = bookingStatuses,
                Stats = weeklyStats,
                ActivityBreakdown = activityBreakdown,
                PeakBookingHours = peakHours
            };
        }

        private async Task<List<BookingStatusDto>> GetBookingStatusesAsync(FilteredReservationDto filter)
        {
            var statuses = new[]
            {
                ReservationStatus.Pending,
                ReservationStatus.Cancelled,
                ReservationStatus.Completed,
                ReservationStatus.Confirmed
            };

            var results = new List<BookingStatusDto>();

            foreach (var status in statuses)
            {
                filter.Status = status;
                var count = await reservationRepo.GetCount(filter);
                results.Add(new BookingStatusDto { Status = status, Count = count });
            }

            filter.Status = null;
            return results;
        }

        private async Task<DashboardStatsDto> GetWeeklyStatsAsync(FilteredReservationDto filter)
        {
            filter.DateFrom = DateTime.Now;
            filter.DateTo = DateTime.Now.AddDays(7);
            filter.Status = null;

            var total = await reservationRepo.GetCount(filter);

            var statusCounts = new Dictionary<ReservationStatus, int>();
            foreach (var status in new[] { ReservationStatus.Cancelled, ReservationStatus.Confirmed, ReservationStatus.Pending })
            {
                filter.Status = status;
                statusCounts[status] = await reservationRepo.GetCount(filter);
            }

            filter.Status = null;
            filter.DateFrom = null;
            filter.DateTo = null;

            return new DashboardStatsDto
            {
                ALLBookings = total,
                RejectedBookings = statusCounts[ReservationStatus.Cancelled],
                AcceptedBooking = statusCounts[ReservationStatus.Confirmed],
                PindingBookings = statusCounts[ReservationStatus.Pending]
            };
        }

        private async Task<List<ActivityBreakdownDto>> GetActivityBreakdownAsync(
            FilteredReservationDto baseFilter, int? userId, int? youthCenterId)
        {
            var activities = await activityService.GetAllAsync();

            if (activities is null or { Count: 0 })
                return [];

            var breakdown = new List<ActivityBreakdownDto>();

            foreach (var activity in activities)
            {
                var count = await reservationRepo.GetActivityCount(new FilteredReservationDto
                {
                    UserId = userId,
                    ActivityId = activity.Id,
                    YouthCenterId = youthCenterId
                });

                breakdown.Add(new ActivityBreakdownDto
                {
                    Count = count,
                    ActivityName = activity.NameAr
                });
            }

            return breakdown;
        }
    }
}
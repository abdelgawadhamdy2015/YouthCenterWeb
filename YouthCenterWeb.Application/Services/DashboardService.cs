using YouthCenterWeb.YouthCenterWeb.Application.Common.helper;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Helpers;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class DashboardService(IReservationRepo repo) : IDashboardService
    {
        public async Task<DashboardOverviewDto> GetDashboardOverviewDtoAsync(int? youthCenterId, int? userId)
        {
            var scope = new FilteredReservationDto { YouthCenterId = youthCenterId, UserId = userId };
            var weekScope = new FilteredReservationDto
            {
                YouthCenterId = youthCenterId,
                UserId = userId,
                DateFrom = CairoClock.Today,
                DateTo = CairoClock.Today.AddDays(7)
            };

            // DbContext واحد: الاستعلامات متتابعة (مش Task.WhenAll)
            var all = await repo.GetStatusCountsAsync(scope);
            var week = await repo.GetStatusCountsAsync(weekScope);
            var breakdown = await repo.GetActivityBreakdownAsync(scope);
            var peak = await repo.GetPeakBookingHoursAsync(scope);

            int C(Dictionary<ReservationStatus, int> d, ReservationStatus s) => d.GetValueOrDefault(s);

            return new DashboardOverviewDto
            {
                AllBookingCount = all.Values.Sum(),
                BookingStatuses = new[]
                {
                    ReservationStatus.Pending, ReservationStatus.Cancelled,
                    ReservationStatus.Completed, ReservationStatus.Confirmed
                }.Select(s => new BookingStatusDto { Status = s, Count = C(all, s) }).ToList(),
                Stats = new DashboardStatsDto
                {
                    ALLBookings = week.Values.Sum(),
                    RejectedBookings = C(week, ReservationStatus.Cancelled),
                    AcceptedBooking = C(week, ReservationStatus.Confirmed),
                    PindingBookings = C(week, ReservationStatus.Pending)
                },
                ActivityBreakdown = breakdown,
                PeakBookingHours = peak
            };
        }
    }
}
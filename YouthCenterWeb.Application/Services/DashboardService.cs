using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class DashboardService(IReservationRepo reservationRepo, IActivityService activityService) : IDashboardService
    {
        public async Task<DashboardOverviewDto> GetDashboardOverviewDtoAsync(int? youthCenterId)
        {
            FilterReservationDashboardDto dashboardDto = new();
            var allBookingCount = await reservationRepo.GetCount(dashboardDto);

            dashboardDto.Status = ReservationStatus.Pending;
            var allPinding = await reservationRepo.GetCount(dashboardDto);

            dashboardDto.Status = ReservationStatus.Completed;
            var allCompleted = await reservationRepo.GetCount(dashboardDto);

            dashboardDto.Status = ReservationStatus.Cancelled;
            var allRejected = await reservationRepo.GetCount(dashboardDto);

            dashboardDto.Status = ReservationStatus.Confirmed;
            var allConfirmed = await reservationRepo.GetCount(dashboardDto);

            var statuses = new List<BookingStatusDto>
            {
                new() { Status = ReservationStatus.Pending, Count = allPinding },
                new() { Status = ReservationStatus.Cancelled, Count = allRejected },
                new() { Status = ReservationStatus.Completed, Count = allCompleted },
                new() { Status = ReservationStatus.Confirmed, Count = allConfirmed }
            };


            dashboardDto.DateFrom = DateTime.Now;
            dashboardDto.DateTo = DateTime.Now.AddDays(7);
            dashboardDto.Status = null;
            var allReservationWeekCount = await reservationRepo.GetCount(dashboardDto);
            dashboardDto.Status = ReservationStatus.Cancelled;
            var rejectedWeekCount = await reservationRepo.GetCount(dashboardDto);
            dashboardDto.Status = ReservationStatus.Confirmed;
            var acceptedWeekCount = await reservationRepo.GetCount(dashboardDto);

            dashboardDto.Status = ReservationStatus.Pending;
            var pindingWeekCount = await reservationRepo.GetCount(dashboardDto);

            var dashboardStatesData = new DashboardStatsDto
            {
                RejectedBookings = rejectedWeekCount,
                ALLBookings = allReservationWeekCount,
                PindingBookings = pindingWeekCount,
                AcceptedBooking = acceptedWeekCount
            };


            var activitiesBreakDown = new List<ActivityBreakdownDto>();
            var activities = await activityService.GetActivitiesByCenter(youthCenterId: (int)youthCenterId!);
            for (int i = 0; i < activities.Count; i++)
            {
                var count = await reservationRepo.GetCount(new FilterReservationDashboardDto
                {
                    ActivityId = activities[i].Id
                });
                activitiesBreakDown.Add(
                  new ActivityBreakdownDto
                  {
                      Count = count,
                      ActivityName = activities[i].NameAr
                  }
                );
            }

            var peakHours = await reservationRepo.GetPeakBookingHoursAsync(null);

            var dashboardData = new DashboardOverviewDto
            {
                Stats = dashboardStatesData,
                BookingStatuses = statuses,
                ActivityBreakdown = activitiesBreakDown,
                AllBookingCount = allBookingCount,
                PeakBookingHours = peakHours

            };
            return dashboardData;
        }
    }
}
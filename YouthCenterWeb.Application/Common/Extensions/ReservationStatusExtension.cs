using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Extensions
{
    public static class ReservationQueryExtensions
    {
        public static IQueryable<Reservation> FilterByCurrentStatus(
            this IQueryable<Reservation> query,
            ReservationStatus status)
        {
            var now = DateTime.Now;

            return query.Where(x =>

                (
                    x.Status == ReservationStatus.Pending &&
                    x.Date.Add(x.EndTime.ToTimeSpan()) < now &&
                    status == ReservationStatus.Cancelled
                )

                ||

                (
                    x.Status == ReservationStatus.Confirmed &&
                    x.Date.Add(x.EndTime.ToTimeSpan()) < now &&
                    status == ReservationStatus.Completed
                )

                ||

                (
                    !(
                        (x.Status == ReservationStatus.Pending ||
                         x.Status == ReservationStatus.Confirmed)
                        &&
                        x.Date.Add(x.EndTime.ToTimeSpan()) < now
                    )
                    &&
                    x.Status == status
                )
            );
        }
    }
}
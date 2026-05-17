using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Data;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services;

public class ReservationStatusBackgroundService(
    IServiceScopeFactory scopeFactory)
        : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var now = DateTime.Now;

                // هات الحجوزات اللي لسه Pending أو Confirmed
                var reservations = await context.Reservations
                    .Where(x =>
                        x.Status == ReservationStatus.Pending ||
                        x.Status == ReservationStatus.Confirmed)
                    .ToListAsync(stoppingToken);

                foreach (var reservation in reservations)
                {
                    // وقت نهاية الحجز
                    var reservationEnd =
                        reservation.Date.Date
                        .Add(reservation.EndTime.ToTimeSpan());

                    // لو الحجز انتهى
                    if (reservationEnd < now)
                    {
                        // Pending -> Cancelled
                        if (reservation.Status ==
                            ReservationStatus.Pending)
                        {
                            reservation.Status =
                                ReservationStatus.Cancelled;
                        }

                        // Confirmed -> Completed
                        else if (reservation.Status ==
                            ReservationStatus.Confirmed)
                        {
                            reservation.Status =
                                ReservationStatus.Completed;
                        }
                    }
                }

                await context.SaveChangesAsync(
                    stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // يتكرر كل ساعة
            await Task.Delay(
                TimeSpan.FromHours(1),
                stoppingToken);
        }
    }
}
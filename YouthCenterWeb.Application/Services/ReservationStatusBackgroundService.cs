using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Application.Common.helper;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class ReservationStatusBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationStatusBackgroundService> logger) : BackgroundService
    {
        private static readonly TimeSpan Tick = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan RolloverEvery = TimeSpan.FromMinutes(10);
        private DateTime _lastRolloverUtc = DateTime.MinValue;

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            using var timer = new PeriodicTimer(Tick);
            do
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IReservationRepo>();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // كل دقيقة
                    await repo.ExpireStaleLocksAsync();

                    // كل 10 دقايق
                    if (DateTime.UtcNow - _lastRolloverUtc >= RolloverEvery)
                    {
                        await RollOverFinishedAsync(context, ct);

                        await context.RefreshTokens
                            .Where(t => t.ExpiresAt < DateTime.UtcNow.AddDays(-7))
                            .ExecuteDeleteAsync(ct);

                        _lastRolloverUtc = DateTime.UtcNow;
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Reservation background job failed");
                }
            }
            while (await timer.WaitForNextTickAsync(ct));
        }

        private static async Task RollOverFinishedAsync(AppDbContext context, CancellationToken ct)
        {
            var now = CairoClock.Now;

            var reservations = await context.Reservations
                .Where(x => (x.Status == ReservationStatus.Pending ||
                             x.Status == ReservationStatus.Confirmed) &&
                            x.Date <= now.Date)               // تقليل الصفوف المحمّلة
                .ToListAsync(ct);

            foreach (var r in reservations)
            {
                if (r.Date.Date.Add(r.EndTime.ToTimeSpan()) >= now) continue;

                r.Status = r.Status == ReservationStatus.Pending
                    ? ReservationStatus.Cancelled
                    : ReservationStatus.Completed;
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
// Infrastructure/Repositories/UserNotificationRepo.cs
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories
{
    public class UserNotificationRepo(AppDbContext db) : IUserNotificationRepo
    {
        public async Task AddAsync(UserNotification n) => await db.Notifications.AddAsync(n);

        public Task AddRangeAsync(IEnumerable<UserNotification> items) =>
            db.Notifications.AddRangeAsync(items);

        public Task<List<UserNotification>> GetByUserAsync(int userId, int take) =>
            db.Notifications.AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();

        public Task<int> CountUnreadAsync(int userId) =>
            db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public Task MarkReadAsync(int userId, int id) =>
            db.Notifications.Where(n => n.Id == id && n.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        public Task MarkAllReadAsync(int userId) =>
            db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        public Task DeleteAsync(int userId, int id) =>
            db.Notifications.Where(n => n.Id == id && n.UserId == userId)
                .ExecuteDeleteAsync();

        public Task<List<int>> GetUserIdsByFavActivityAsync(int activityId) =>
            db.Users.Where(u => u.ActivityId == activityId && !u.IsDeleted)
                .Select(u => u.Id).ToListAsync();

        public Task SaveChangesAsync() => db.SaveChangesAsync();
    }
}
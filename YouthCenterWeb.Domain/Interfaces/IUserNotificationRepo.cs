using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces
{
    public interface IUserNotificationRepo
    {
        Task AddAsync(UserNotification n);
        Task AddRangeAsync(IEnumerable<UserNotification> items);
        Task<int> CountUnreadAsync(int userId);
        Task MarkReadAsync(int userId, int id);
        Task MarkAllReadAsync(int userId);
        Task DeleteAsync(int userId, int id);
        Task<List<int>> GetUserIdsByFavActivityAsync(int activityId);
        Task<List<UserNotification>> GetByUserAsync(int userId, int skip, int take);
        Task<int> CountAllAsync(int userId);
        Task SaveChangesAsync();
    }
}
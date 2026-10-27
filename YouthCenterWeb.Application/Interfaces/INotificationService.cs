using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendToUserAsync(int userId, string title, string body,
            Dictionary<string, string>? data = null,
            string? type = null, int? targetId = null);

        Task SendToTopicAsync(string topic, string title, string body,
            Dictionary<string, string>? data = null);

        Task BroadcastToActivityAsync(int activityId, string topic, string title,
            string body, string type, int? targetId,
            Dictionary<string, string>? data = null);

        Task RegisterDeviceAsync(int userId, string token, string platform);
        Task UnregisterDeviceAsync(int userId);

        Task<List<NotificationDto>> GetMineAsync(int userId, int take = 50);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkReadAsync(int userId, int id);
        Task MarkAllReadAsync(int userId);
        Task DeleteAsync(int userId, int id);
    }
}
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

public interface INotificationService
{
    // الـ data بقت اختيارية، وأضفنا type/targetId (باقي الاستدعاءات القديمة تفضل شغالة)
    Task SendToUserAsync(int userId, string title, string body,
        Dictionary<string, string>? data = null,
        string? type = null, int? targetId = null);

    Task SendToTopicAsync(string topic, string title, string body,
        Dictionary<string, string>? data = null);

    // بث لمتابعي نشاط معيّن: يحفظ لكل مستخدم + push على الـ topic
    Task BroadcastToActivityAsync(int activityId, string topic, string title,
        string body, string type, int? targetId, Dictionary<string, string>? data = null);

    Task RegisterDeviceAsync(int userId, string token, string platform);
    Task UnregisterDeviceAsync(int userId);

    // ── قراءة (للـ Controller) ──
    Task<List<NotificationDto>> GetMineAsync(int userId, int take = 50);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkReadAsync(int userId, int id);
    Task MarkAllReadAsync(int userId);
    Task DeleteAsync(int userId, int id);
}
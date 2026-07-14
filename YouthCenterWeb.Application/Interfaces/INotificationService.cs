namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendToUserAsync(int userId, string title, string body, Dictionary<string, string>? data = null);
        Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string>? data = null);
        Task RegisterDeviceAsync(int userId, string token, string platform);
        Task UnregisterDeviceAsync(int userId);   // ← new
    }
}
namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces
{
    public interface INotificationSender
    {
        Task SendAsync(int userId, string title, string body, string type, int? targetId);
    }
}
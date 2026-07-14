namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities;

public class NotificationMessage
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Dictionary<string, string>? Data { get; set; }
}
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;


public interface IPushNotificationSender
{
    Task<MulticastSendResult> SendToTokensAsync(List<string> tokens, NotificationMessage message);
    Task SendToTopicAsync(string topic, NotificationMessage message);
}

public class MulticastSendResult
{
    public List<TokenSendResult> Results { get; set; } = new();
}

public class TokenSendResult
{
    public string Token { get; set; } = null!;
    public bool Success { get; set; }
    public bool IsInvalidToken { get; set; } // عشان نعرف نمسحه ولا لا
}
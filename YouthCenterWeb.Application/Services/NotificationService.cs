// Application/Services/NotificationService.cs
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services;


public class NotificationService(
    IDeviceTokenRepository tokenRepository,
    IPushNotificationSender pushSender) : INotificationService
{
    private readonly IDeviceTokenRepository _tokenRepository = tokenRepository;
    private readonly IPushNotificationSender _pushSender = pushSender;

    public async Task SendToUserAsync(
        int userId, string title, string body, Dictionary<string, string>? data = null)
    {
        var tokens = await _tokenRepository.GetTokensByUserIdAsync(userId);
        if (tokens.Count == 0) return;

        var payloadData = data ?? new Dictionary<string, string>();
        payloadData["click_action"] = "FLUTTER_NOTIFICATION_CLICK";

        var message = new NotificationMessage { Title = title, Body = body, Data = payloadData };
        var result = await _pushSender.SendToTokensAsync(tokens, message);

        // نظّف أي توكن بقى غير صالح
        foreach (var tokenResult in result.Results)
        {
            if (!tokenResult.Success && tokenResult.IsInvalidToken)
            {
                await _tokenRepository.RemoveTokenAsync(tokenResult.Token);
            }
        }

        await _tokenRepository.SaveChangesAsync();
    }

    public async Task SendToTopicAsync(
        string topic, string title, string body, Dictionary<string, string>? data = null)
    {
        var message = new NotificationMessage { Title = title, Body = body, Data = data };
        await _pushSender.SendToTopicAsync(topic, message);
    }

    public async Task RegisterDeviceAsync(int userId, string token, string platform)
    {
        await _tokenRepository.AddOrUpdateAsync(userId, token, platform);
        await _tokenRepository.SaveChangesAsync();
    }

    public async Task UnregisterDeviceAsync(int userId)
    {
        await _tokenRepository.RemoveTokenByUserIdAsync(userId);
        await _tokenRepository.SaveChangesAsync();
    }
}
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services;

public class NotificationService(
    IDeviceTokenRepository tokenRepository,
    IPushNotificationSender pushSender,
    IUserNotificationRepo notificationRepo,
    ILogger<NotificationService> logger) : INotificationService
{
    private static Dictionary<string, string> BuildData(
        Dictionary<string, string>? data, string? type, int? targetId)
    {
        var d = data is null ? new() : new Dictionary<string, string>(data);
        if (type != null) d["type"] = type;
        if (targetId != null) d["target_id"] = targetId.Value.ToString();
        d["click_action"] = "FLUTTER_NOTIFICATION_CLICK";
        return d;
    }

    // ── إرسال ────────────────────────────────────────────────────────────────
    public async Task SendToUserAsync(int userId, string title, string body,
        Dictionary<string, string>? data = null,
        string? type = null, int? targetId = null)
    {
        // 1) احفظ الأول (حتى لو مفيش توكن)
        await notificationRepo.AddAsync(new UserNotification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            TargetId = targetId,
            CreatedAt = DateTime.UtcNow
        });
        await notificationRepo.SaveChangesAsync();

        // 2) push: أي فشل هنا ميكسرش العملية الأساسية
        try
        {
            var tokens = await tokenRepository.GetTokensByUserIdAsync(userId);
            if (tokens.Count == 0) return;

            var message = new NotificationMessage
            {
                Title = title,
                Body = body,
                Data = BuildData(data, type, targetId)
            };

            var result = await pushSender.SendToTokensAsync(tokens, message);

            foreach (var r in result.Results)
                if (!r.Success && r.IsInvalidToken)
                    await tokenRepository.RemoveTokenAsync(r.Token);

            await tokenRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Push failed for user {UserId}", userId);
        }
    }

    public async Task SendToTopicAsync(string topic, string title, string body,
        Dictionary<string, string>? data = null)
    {
        try
        {
            await pushSender.SendToTopicAsync(topic,
                new NotificationMessage { Title = title, Body = body, Data = data });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Topic push failed {Topic}", topic);
        }
    }

    public async Task BroadcastToActivityAsync(int activityId, string topic,
        string title, string body, string type, int? targetId,
        Dictionary<string, string>? data = null)
    {
        var userIds = await notificationRepo.GetUserIdsByFavActivityAsync(activityId);
        var now = DateTime.UtcNow;

        await notificationRepo.AddRangeAsync(userIds.Select(id => new UserNotification
        {
            UserId = id,
            Title = title,
            Body = body,
            Type = type,
            TargetId = targetId,
            CreatedAt = now
        }));
        await notificationRepo.SaveChangesAsync();

        await SendToTopicAsync(topic, title, body, BuildData(data, type, targetId));
    }

    // ── الأجهزة ──────────────────────────────────────────────────────────────
    public async Task RegisterDeviceAsync(int userId, string token, string platform)
    {
        await tokenRepository.AddOrUpdateAsync(userId, token, platform);
        await tokenRepository.SaveChangesAsync();
    }

    public async Task UnregisterDeviceAsync(int userId)
    {
        await tokenRepository.RemoveTokenByUserIdAsync(userId);
        await tokenRepository.SaveChangesAsync();
    }

    // ── قراءة ────────────────────────────────────────────────────────────────
    public async Task<List<NotificationDto>> GetMineAsync(int userId, int take = 50) =>
        (await notificationRepo.GetByUserAsync(userId, take))
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Body = n.Body,
                Type = n.Type,
                TargetId = n.TargetId,
                IsRead = n.IsRead,
                // EF بيرجّع Kind=Unspecified؛ نحدده UTC عشان الـ App يحوّله لتوقيت الجهاز صح
                CreatedAt = DateTime.SpecifyKind(n.CreatedAt, DateTimeKind.Utc)
            })
            .ToList();

    public Task<int> GetUnreadCountAsync(int userId) =>
        notificationRepo.CountUnreadAsync(userId);

    public Task MarkReadAsync(int userId, int id) =>
        notificationRepo.MarkReadAsync(userId, id);

    public Task MarkAllReadAsync(int userId) =>
        notificationRepo.MarkAllReadAsync(userId);

    public Task DeleteAsync(int userId, int id) =>
        notificationRepo.DeleteAsync(userId, id);
}
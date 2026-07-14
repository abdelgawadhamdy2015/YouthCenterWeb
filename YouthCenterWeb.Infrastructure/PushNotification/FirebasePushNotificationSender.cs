using FirebaseAdmin.Messaging;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.PushNotification;


public class FirebasePushNotificationSender : IPushNotificationSender
{
    public async Task<MulticastSendResult> SendToTokensAsync(List<string> tokens, NotificationMessage message)
    {
        var fcmMessage = new MulticastMessage
        {
            Tokens = tokens, // ✅ ده اللي كان ناقص
            Notification = new Notification { Title = message.Title, Body = message.Body },
            Data = message.Data,
        };

        var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(fcmMessage);

        var result = new MulticastSendResult();
        for (int i = 0; i < response.Responses.Count; i++)
        {
            var res = response.Responses[i];
            result.Results.Add(new TokenSendResult
            {
                Token = tokens[i],
                Success = res.IsSuccess,
                IsInvalidToken = !res.IsSuccess &&
                    (res.Exception?.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                     res.Exception?.MessagingErrorCode == MessagingErrorCode.InvalidArgument),
            });
        }
        return result;
    }



    public async Task SendToTopicAsync(string topic, NotificationMessage message)
    {
        var fcmMessage = new Message
        {
            Topic = topic,
            Notification = new Notification { Title = message.Title, Body = message.Body },
            Data = message.Data,
        };
        await FirebaseMessaging.DefaultInstance.SendAsync(fcmMessage);
    }


}
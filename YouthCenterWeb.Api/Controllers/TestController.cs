// Controllers/TestController.cs — احذفه بعد ما تخلص اختبار
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IPushNotificationSender _pushSender;

    public TestController(IPushNotificationSender pushSender)
    {
        _pushSender = pushSender;
    }

    public record TestNotificationRequest(string Token, string Title, string Body);

    [HttpPost("notification")]
    public async Task<IActionResult> SendTest(TestNotificationRequest req)
    {
        var message = new NotificationMessage { Title = req.Title, Body = req.Body };
        var result = await _pushSender.SendToTokensAsync(
            new List<string> { req.Token }, message);

        return Ok(result);
    }
}
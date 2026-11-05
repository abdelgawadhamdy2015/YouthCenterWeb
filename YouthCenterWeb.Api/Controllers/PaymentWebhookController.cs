using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers;

[ApiController]
[Route("api/webhooks/payment")]
public class PaymentWebhookController(
    IReservationService reservationService,
    IPaymentSignatureVerifier signatureVerifier,
    ILogger<PaymentWebhookController> logger) : ControllerBase
{
    [HttpPost("paymob")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymobCallback(
    [FromBody] PaymobWebhookDto payload, [FromQuery] string? hmac)
    {
        if (!signatureVerifier.IsValid(payload, hmac ?? payload.Hmac))
        {
            logger.LogWarning("Invalid Paymob signature");
            return Unauthorized();
        }

        var obj = payload.Obj;
        // أي حاجة مش معاملة نهائية: نرد 200 ونتجاهلها
        if (payload.Type != "TRANSACTION" || obj is null ||
            obj.Pending || obj.IsVoided || obj.IsRefunded || obj.HasParentTransaction)
            return Ok();

        if (!TryParseReservationId(obj.Order?.MerchantOrderId, out var reservationId))
        {
            logger.LogError("Bad MerchantOrderId: {Id}", obj.Order?.MerchantOrderId);
            return Ok(); // نرد 200 عشان Paymob مايعيدش الطلب للأبد
        }

        try
        {
            if (obj.Success)
                await reservationService.ConfirmPaymentAsync(reservationId, new ConfirmPaymentDto
                {
                    TransactionId = obj.Id.ToString(),
                    AmountPaid = obj.AmountCents / 100m,
                    PaymentMethod = obj.SourceData?.Type,
                    GatewayReference = obj.Order?.Id.ToString()
                });
            else
                await reservationService.FailPaymentAsync(reservationId, "Payment declined by gateway");
        }
        catch (BusinessException ex)
        {
            logger.LogWarning(ex, "Webhook business error for reservation {Id}", reservationId);
        }
        return Ok();
    }

    private static bool TryParseReservationId(string? merchantOrderId, out int id)
    {
        id = 0;
        var parts = merchantOrderId?.Split('-');
        return parts is { Length: 3 } && parts[0] == "RES" && int.TryParse(parts[1], out id);
    }
}
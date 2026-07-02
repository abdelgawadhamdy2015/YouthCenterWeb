using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Interfaces;
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
    [AllowAnonymous] // explicit — no user JWT involved
    public async Task<IActionResult> PaymobCallback([FromBody] PaymobWebhookDto payload)
    {
        // 1. Verify the signature BEFORE trusting anything in the payload
        if (!signatureVerifier.IsValid(payload))
        {
            logger.LogWarning("Invalid Paymob webhook signature. OrderId={OrderId}", payload.Obj?.Order?.Id);
            return Unauthorized();
        }

        // 2. Map gateway's reservation reference back to your reservation id
        var reservationId = payload.Obj?.Order?.MerchantOrderId; // however you passed it when creating the payment

        if (payload.Obj?.Success == true)
        {
            await reservationService.ConfirmPaymentAsync(
                int.Parse(reservationId!),
                new ConfirmPaymentDto
                {
                    TransactionId = payload.Obj.Id.ToString(),
                    AmountPaid = payload.Obj.AmountCents / 100m,
                    PaymentMethod = payload.Obj.SourceData?.Type,
                    GatewayReference = payload.Obj.Order?.Id.ToString()
                });
        }
        else
        {
            await reservationService.FailPaymentAsync(int.Parse(reservationId!), "Payment declined by gateway");
        }

        // 3. Gateways expect a 200 quickly, or they'll retry the webhook
        return Ok();
    }
}
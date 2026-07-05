using System.Net.Http.Json;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services;

public class PaymobPaymentGateway(HttpClient httpClient, IConfiguration config) : IPaymentGateway
{
    public async Task<PaymentOrderResult> CreatePaymentOrderAsync(
        string merchantOrderId, int amountCents, User? payer = null)
    {
        var apiKey = config["Paymob:ApiKey"]
            ?? throw new InvalidOperationException("Paymob:ApiKey is not configured.");
        var integrationId = config["Paymob:IntegrationId"]
            ?? throw new InvalidOperationException("Paymob:IntegrationId is not configured.");
        var iframeId = config["Paymob:IframeId"]
            ?? throw new InvalidOperationException("Paymob:IframeId is not configured.");

        // 1) Auth — exchange the API key for a short-lived token
        var authResponse = await httpClient.PostAsJsonAsync(
            "https://accept.paymob.com/api/auth/tokens",
            new { api_key = apiKey });
        authResponse.EnsureSuccessStatusCode();
        var authResult = await authResponse.Content.ReadFromJsonAsync<PaymobAuthResponse>()
            ?? throw new InvalidOperationException("Paymob auth response was empty.");

        // 2) Order registration
        var orderResponse = await httpClient.PostAsJsonAsync(
            "https://accept.paymob.com/api/ecommerce/orders",
            new
            {
                auth_token = authResult.Token,
                delivery_needed = false,
                amount_cents = amountCents,
                currency = "EGP",
                merchant_order_id = merchantOrderId,
                items = Array.Empty<object>()
            });
        orderResponse.EnsureSuccessStatusCode();
        var orderResult = await orderResponse.Content.ReadFromJsonAsync<PaymobOrderResponse>()
            ?? throw new InvalidOperationException("Paymob order response was empty.");

        // 3) Payment key — carries the real billing data
        var paymentKeyResponse = await httpClient.PostAsJsonAsync(
            "https://accept.paymob.com/api/acceptance/payment_keys",
            new
            {
                auth_token = authResult.Token,
                amount_cents = amountCents,
                expiration = 600, // keep in sync with PaymentLockExpiresAt (10 min)
                order_id = orderResult.Id,
                billing_data = new
                {
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "NA",
                    country = "EG",
                    state = "NA",
                    first_name = payer?.Name ?? "NA",
                    last_name = payer?.Name ?? "NA",
                    phone_number = payer?.Mobile ?? "+201000000000",
                    email = payer?.Email ?? "customer@example.com"
                },
                currency = "EGP",
                integration_id = int.Parse(integrationId)
            });
        paymentKeyResponse.EnsureSuccessStatusCode();
        var paymentKeyResult = await paymentKeyResponse.Content.ReadFromJsonAsync<PaymobPaymentKeyResponse>()
            ?? throw new InvalidOperationException("Paymob payment key response was empty.");

        var checkoutUrl =
            $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentKeyResult.Token}";

        return new PaymentOrderResult(orderResult.Id.ToString(), checkoutUrl);
    }

    private record PaymobAuthResponse(string Token);
    private record PaymobOrderResponse(long Id);
    private record PaymobPaymentKeyResponse(string Token);
}
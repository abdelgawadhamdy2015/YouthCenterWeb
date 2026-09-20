using System.Net.Http.Json;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services;

public class PaymobPaymentGateway(
    HttpClient httpClient,
    IConfiguration config) : IPaymentGateway
{
    private const string BaseUrl = "https://accept.paymob.com/api";

    public async Task<PaymentOrderResult> CreatePaymentOrderAsync(
        string merchantOrderId,
        int amountCents,
        User? payer = null)
    {
        var authToken = await GetAuthTokenAsync();

        var orderResult = await CreateOrderAsync(
            authToken,
            merchantOrderId,
            amountCents);

        return await CreatePaymentKeyAsync(
            authToken,
            orderResult.Id,
            amountCents,
            payer);
    }

    /// <summary>
    /// Creates a payment key for an existing Paymob order.
    /// Used when the reservation already has a Paymob order.
    /// </summary>
    public async Task<PaymentOrderResult> CreatePaymentForExistingOrderAsync(
        string paymobOrderId,
        int amountCents,
        User? payer = null)
    {
        var authToken = await GetAuthTokenAsync();

        if (!long.TryParse(paymobOrderId, out var orderId))
        {
            throw new PaymobException(
                400,
                "Invalid Paymob order ID.",
                $"Invalid Paymob order ID: {paymobOrderId}");
        }

        return await CreatePaymentKeyAsync(
            authToken,
            orderId,
            amountCents,
            payer);
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var apiKey = config["Paymob:ApiKey"]
            ?? throw new InvalidOperationException(
                "Paymob:ApiKey is not configured.");

        var response = await httpClient.PostAsJsonAsync(
            $"{BaseUrl}/auth/tokens",
            new
            {
                api_key = apiKey
            });

        var body = await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"Paymob Auth Response: Status={(int)response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            throw new PaymobException(
                (int)response.StatusCode,
                "Paymob authentication failed.",
                body);
        }

        var result = await response.Content
            .ReadFromJsonAsync<PaymobAuthResponse>();

        if (result == null || string.IsNullOrWhiteSpace(result.Token))
        {
            throw new PaymobException(
                500,
                "Paymob authentication token was empty.",
                body);
        }

        return result.Token;
    }

    private async Task<PaymobOrderResponse> CreateOrderAsync(
        string authToken,
        string merchantOrderId,
        int amountCents)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"{BaseUrl}/ecommerce/orders",
            new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = amountCents,
                currency = "EGP",
                merchant_order_id = merchantOrderId,
                items = Array.Empty<object>()
            });

        var body = await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"Paymob Create Order Response: Status={(int)response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            throw new PaymobException(
                (int)response.StatusCode,
                "Paymob order creation failed.",
                body);
        }

        var result = await response.Content
            .ReadFromJsonAsync<PaymobOrderResponse>();

        if (result == null)
        {
            throw new PaymobException(
                500,
                "Paymob order response was empty.",
                body);
        }

        return result;
    }

    private async Task<PaymentOrderResult> CreatePaymentKeyAsync(
        string authToken,
        long orderId,
        int amountCents,
        User? payer)
    {
        var integrationId = config["Paymob:IntegrationId"]
            ?? throw new InvalidOperationException(
                "Paymob:IntegrationId is not configured.");

        var iframeId = config["Paymob:IframeId"]
            ?? throw new InvalidOperationException(
                "Paymob:IframeId is not configured.");

        var response = await httpClient.PostAsJsonAsync(
            $"{BaseUrl}/acceptance/payment_keys",
            new
            {
                auth_token = authToken,
                amount_cents = amountCents,
                expiration = 3600,
                order_id = orderId,
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
                    first_name = payer?.Name ?? "Customer",
                    last_name = payer?.Name ?? "Customer",
                    phone_number =
                        payer?.Mobile ?? "+201000000000",
                    email =
                        payer?.Email ?? "customer@example.com"
                },
                currency = "EGP",
                integration_id = int.Parse(integrationId)
            });

        var body = await response.Content.ReadAsStringAsync();

        Console.WriteLine(
            $"Paymob Payment Key Response: Status={(int)response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            throw new PaymobException(
                (int)response.StatusCode,
                "Paymob payment key creation failed.",
                body);
        }

        var result = await response.Content
            .ReadFromJsonAsync<PaymobPaymentKeyResponse>();

        if (result == null ||
            string.IsNullOrWhiteSpace(result.Token))
        {
            throw new PaymobException(
                500,
                "Paymob payment key was empty.",
                body);
        }

        var checkoutUrl =
            $"{BaseUrl}/acceptance/iframes/{iframeId}" +
            $"?payment_token={result.Token}";

        return new PaymentOrderResult(
            orderId.ToString(),
            checkoutUrl);
    }

    private record PaymobAuthResponse(string Token);

    private record PaymobOrderResponse(long Id);

    private record PaymobPaymentKeyResponse(string Token);

    public class PaymobException : Exception
    {
        public int StatusCode { get; }

        public string ResponseBody { get; }

        public PaymobException(
            int statusCode,
            string message,
            string responseBody)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
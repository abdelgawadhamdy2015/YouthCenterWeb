using System.Security.Cryptography;
using System.Text;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services;

public class PaymobSignatureVerifier(IConfiguration config) : IPaymentSignatureVerifier
{
    public bool IsValid(PaymobWebhookDto payload)
    {
        var hmacSecret = config["Paymob:HmacSecret"]!; // from appsettings / secret manager, never hardcoded

        // Paymob concatenates specific fields in a FIXED order — check their current docs for the exact list,
        // this is illustrative of the pattern:
        var concatenated = string.Concat(
            payload.Obj?.AmountCents,
            payload.Obj?.CreatedAt,
            payload.Obj?.Currency,
            payload.Obj?.ErrorOccured,
            payload.Obj?.Id,
            payload.Obj?.IntegrationId,
            payload.Obj?.Success,
            payload.Obj?.Order?.Id
        // ...etc per Paymob's documented field order
        );

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hmacSecret));
        var computedHash = Convert.ToHexString(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenated))).ToLower();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(payload.Hmac ?? ""));
    }
}
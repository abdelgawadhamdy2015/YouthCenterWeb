using System.Security.Cryptography;
using System.Text;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services;

public class PaymobSignatureVerifier(IConfiguration config) : IPaymentSignatureVerifier
{
    public bool IsValid(PaymobWebhookDto payload, string? hmac)
    {
        var o = payload.Obj;
        if (o is null || string.IsNullOrEmpty(hmac)) return false;

        var secret = config["Paymob:HmacSecret"];
        if (string.IsNullOrWhiteSpace(secret)) return false;

        static string B(bool v) => v ? "true" : "false";

        // الترتيب الرسمي من Paymob (أبجدي)
        var concatenated = string.Concat(
            o.AmountCents,
            o.CreatedAt,
            o.Currency,
            B(o.ErrorOccured),
            B(o.HasParentTransaction),
            o.Id,
            o.IntegrationId,
            B(o.Is3dSecure),
            B(o.IsAuth),
            B(o.IsCapture),
            B(o.IsRefunded),
            B(o.IsStandalonePayment),
            B(o.IsVoided),
            o.Order?.Id,
            o.Owner,
            B(o.Pending),
            o.SourceData?.Pan,
            o.SourceData?.SubType,
            o.SourceData?.Type,
            B(o.Success));

        using var h = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
        var computed = Convert.ToHexString(h.ComputeHash(Encoding.UTF8.GetBytes(concatenated))).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(hmac.ToLowerInvariant()));
    }

}
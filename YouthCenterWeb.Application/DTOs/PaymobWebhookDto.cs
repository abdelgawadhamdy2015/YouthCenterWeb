using System.Text.Json.Serialization;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class PaymobWebhookDto
    {
        public string? Type { get; set; }
        public PaymobTransactionObj? Obj { get; set; }
        public string? Hmac { get; set; }
    }

    public class PaymobTransactionObj
    {
        public long Id { get; set; }
        public bool Pending { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        public bool Success { get; set; }

        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("is_3d_secure")]
        public bool Is3dSecure { get; set; }

        // كان ناقص — Paymob بيرجعه للمعاملات اللي عليها refund/capture مرتبط بمعاملة تانية قبلها
        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonPropertyName("integration_id")]
        public int? IntegrationId { get; set; }

        [JsonPropertyName("error_occured")]
        public bool ErrorOccured { get; set; }

        [JsonPropertyName("refunded_amount_cents")]
        public int RefundedAmountCents { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        public string? Currency { get; set; }

        // كان ناقص — ده الحساب بتاع الـ merchant اللي استقبل الفلوس، مش user id بتاعك
        // Paymob بيرجعه كـ object فيه id, email, ... — لو مش محتاج تفاصيله كلها خده كـ int? للـ id بس
        [JsonPropertyName("owner")]
        public int? Owner { get; set; }

        [JsonPropertyName("source_data")]
        public PaymobSourceData? SourceData { get; set; }

        public PaymobOrder? Order { get; set; }
    }

    public class PaymobSourceData
    {
        public string? Type { get; set; }

        [JsonPropertyName("sub_type")]
        public string? SubType { get; set; }

        public string? Pan { get; set; }
    }

    public class PaymobOrder
    {
        public long Id { get; set; }

        [JsonPropertyName("merchant_order_id")]
        public string? MerchantOrderId { get; set; }
    }
}
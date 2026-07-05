namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs;

public class ConfirmPaymentDto
{
    public string TransactionId { get; set; } = string.Empty;

    // Optional but recommended — store what the gateway actually charged/returned,
    // so you can cross-check against reservation.TotalPrice before confirming.
    public decimal? AmountPaid { get; set; }

    public string? PaymentMethod { get; set; } // e.g. "card", "wallet", "fawry"

    public string? GatewayReference { get; set; } // gateway's own order/txn id, for audit trail
}
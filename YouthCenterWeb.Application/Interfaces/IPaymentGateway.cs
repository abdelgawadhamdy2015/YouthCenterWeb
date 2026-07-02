using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentOrderResult> CreatePaymentOrderAsync(string merchantOrderId, int amountCents, User? payer = null);
}
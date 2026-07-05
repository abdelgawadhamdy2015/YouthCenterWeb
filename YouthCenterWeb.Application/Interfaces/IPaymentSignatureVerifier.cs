using System.Security.Cryptography;
using System.Text;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

public interface IPaymentSignatureVerifier
{
    bool IsValid(PaymobWebhookDto payload);
}

using YouthCenterWeb.YouthCenterWeb.Application.Common.Exceptions;

namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;


public class BusinessException(string messageAr, string messageEn) : Exception(messageEn)
{
    public string MessageAr { get; } = messageAr;
    public string MessageEn { get; } = messageEn;

    // Convenience: single-language (En) — Arabic falls back to generic
    public BusinessException(string messageEn)
        : this(ExceptionMessages.Business.Ar, messageEn) { }
}
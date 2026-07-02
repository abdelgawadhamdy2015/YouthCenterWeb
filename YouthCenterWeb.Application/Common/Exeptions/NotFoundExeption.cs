// Application/Common/Exceptions/NotFoundException.cs
namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;

public class NotFoundException : Exception
{
    public string MessageAr { get; }
    public string MessageEn { get; }

    public NotFoundException(string entity, object key)
        : base($"{entity} ({key}) was not found.")
    {
        MessageAr = $"لم يتم العثور على {entity} بالمعرف ({key}).";
        MessageEn = $"{entity} with ID ({key}) was not found.";
    }

    public NotFoundException(string messageAr, string messageEn)
        : base(messageEn)
    {
        MessageAr = messageAr;
        MessageEn = messageEn;
    }
}
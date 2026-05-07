// Core/Exceptions/BusinessException.cs
namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;

public class BusinessException : Exception
{


    public BusinessException(string message)
        : base(message)
    {
    }
}
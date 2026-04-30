// Core/Exceptions/BusinessException.cs
namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;

public class BusinessException(string message) : Exception(message)
{
}
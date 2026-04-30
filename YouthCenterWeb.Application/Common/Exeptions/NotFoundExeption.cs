// Core/Exceptions/NotFoundException.cs
namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entity, object id)
        : base($"{entity} with id '{id}' was not found.") { }
}
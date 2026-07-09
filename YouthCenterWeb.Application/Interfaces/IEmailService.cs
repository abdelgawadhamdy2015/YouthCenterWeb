namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string email, string subject, string message);
    }
}
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces
{
    public interface IOtpRepo
    {
        Task<OtpEntry?> GetLatestAsync(string email);
        Task<bool> IsVerifiedAsync(string email);
        Task AddAsync(OtpEntry entry);
        Task DeleteAllForEmailAsync(string email);
        Task SaveChangesAsync();
    }
}
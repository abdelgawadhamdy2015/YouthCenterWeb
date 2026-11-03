using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

public interface IRefreshTokenRepo
{
    Task<RefreshToken?> GetByHashAsync(string hash);
    Task AddAsync(RefreshToken token);
    /// يلغي التوكن ذريًا. false = أحد سبقك (طلب متزامن).
    Task<bool> TryRevokeAsync(int id, string replacedByHash);
    Task RevokeAllForUserAsync(int userId);
    Task DeleteOldAsync();
    Task SaveChangesAsync();
}
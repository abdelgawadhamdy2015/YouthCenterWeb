using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

public class RefreshTokenRepo(AppDbContext context) : IRefreshTokenRepo
{
    public Task<RefreshToken?> GetByHashAsync(string hash) =>
        context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(t => t.TokenHash == hash);

    public async Task AddAsync(RefreshToken token) =>
        await context.RefreshTokens.AddAsync(token);

    public async Task<bool> TryRevokeAsync(int id, string replacedByHash)
    {
        var now = DateTime.UtcNow;
        var rows = await context.RefreshTokens
            .Where(t => t.Id == id && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAt, now)
                .SetProperty(t => t.ReplacedByHash, replacedByHash));
        return rows == 1;
    }

    public Task RevokeAllForUserAsync(int userId)
    {
        var now = DateTime.UtcNow;
        return context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now));
    }

    public Task DeleteOldAsync()
    {
        var limit = DateTime.UtcNow.AddDays(-7);
        return context.RefreshTokens.Where(t => t.ExpiresAt < limit).ExecuteDeleteAsync();
    }

    public Task SaveChangesAsync() => context.SaveChangesAsync();
}
// Infrastructure/Repositories/DeviceTokenRepository.cs
namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

using global::YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using global::YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using global::YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class DeviceTokenRepository : IDeviceTokenRepository
{
    private readonly AppDbContext context;
    public DeviceTokenRepository(AppDbContext db) => context = db;

    public async Task<List<string>> GetTokensByUserIdAsync(int userId)
    {
        return await context.DeviceTokens
            .Where(t => t.UserId == userId)
            .Select(t => t.Token)
            .ToListAsync();
    }
    public async Task AddOrUpdateAsync(int userId, string token, string platform)
    {
        var tokenOwnedByOther = await context.DeviceTokens
            .FirstOrDefaultAsync(t => t.Token == token && t.UserId != userId);
        if (tokenOwnedByOther != null)
            context.DeviceTokens.Remove(tokenOwnedByOther);

        var existingForUser = await context.DeviceTokens
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (existingForUser != null)
        {
            existingForUser.Token = token;
            existingForUser.Platform = platform;
            existingForUser.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            context.DeviceTokens.Add(new DeviceToken
            {
                UserId = userId,
                Token = token,
                Platform = platform,
                UpdatedAt = DateTime.UtcNow,
            });
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException) when (existingForUser == null)
        {
            // Another request beat us to inserting this user's row — retry as an update.
            context.ChangeTracker.Clear();
            var nowExisting = await context.DeviceTokens.FirstAsync(t => t.UserId == userId);
            nowExisting.Token = token;
            nowExisting.Platform = platform;
            nowExisting.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
    public async Task RemoveTokenByUserIdAsync(int userId)
    {
        var existing = await context.DeviceTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();

        if (existing.Count > 0)
            context.DeviceTokens.RemoveRange(existing);
    }

    public async Task RemoveTokenAsync(string token)
    {
        var existing = await context.DeviceTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (existing != null) context.DeviceTokens.Remove(existing);
    }
    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}
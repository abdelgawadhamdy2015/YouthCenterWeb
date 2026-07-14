public interface IDeviceTokenRepository
{
    Task<List<string>> GetTokensByUserIdAsync(int userId);
    Task AddOrUpdateAsync(int userId, string token, string platform);
    Task RemoveTokenAsync(string token);              // still used for invalid-token cleanup
    Task RemoveTokenByUserIdAsync(int userId);         // ← new, used on logout
    Task SaveChangesAsync();
}
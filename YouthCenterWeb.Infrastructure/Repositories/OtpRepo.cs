using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories
{
    public class OtpRepo(AppDbContext context) : IOtpRepo
    {
        private readonly AppDbContext _context = context;

        public Task<OtpEntry?> GetLatestAsync(string email) =>
            _context.OtpEntries
                .Where(o => o.Email == email && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

        public Task<bool> IsVerifiedAsync(string email) =>
            _context.OtpEntries
                .AnyAsync(o => o.Email == email &&
                               o.IsVerified == true &&
                               o.ExpiresAt > DateTime.UtcNow);

        public async Task AddAsync(OtpEntry entry) =>
            await _context.OtpEntries.AddAsync(entry);

        public async Task DeleteAllForEmailAsync(string email)
        {
            var entries = await _context.OtpEntries
                .Where(o => o.Email == email)
                .ToListAsync();
            _context.OtpEntries.RemoveRange(entries);
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
        public Task<OtpEntry?> GetLatestAnyAsync(string email) =>
    _context.OtpEntries
        .Where(o => o.Email == email)
        .OrderByDescending(o => o.CreatedAt)
        .FirstOrDefaultAsync();
    }
}
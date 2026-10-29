using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories
{


    public class AuthRepo(AppDbContext context) : IAuthRepo
    {
        private readonly AppDbContext _context = context;

        public async Task<User?> GetByEmailAsync(string email) =>
            await _context.Users.AsQueryable()
                .Include(u => u.Activity)
                .Include(u => u.YouthCenter)
                .Include(u => u.Reservations)
                .FirstOrDefaultAsync(x => x.Email == email);

        public async Task AddAsync(User user) => await _context.Users.AddAsync(user);

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public Task UpdateSessionAsync(User user, Guid sessionId)
        {
            user.CurrentSessionId = sessionId;
            _context.Users.Update(user);
            return _context.SaveChangesAsync();
        }
        public async Task UpdatePasswordAsync(User user, string hashedPassword)
        {
            user.HashedPassword = hashedPassword;
            user.CurrentSessionId = Guid.NewGuid();
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
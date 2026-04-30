using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Data;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories
{
    public class AuthRepo(AppDbContext context) : IAuthRepo
    {
        private readonly AppDbContext _context = context;

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.AsAsyncEnumerable().FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }


    }
}
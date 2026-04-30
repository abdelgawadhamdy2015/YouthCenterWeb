using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Data;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories
{
    public class UserRepo(AppDbContext context) : IUserRepo
    {
        private readonly AppDbContext _context = context;

        private IQueryable<User> WithIncludes(
            params Expression<Func<User, object>>[] includes)
        {
            IQueryable<User> query = _context.Users;
            foreach (var include in includes)
                query = query.Include(include);
            return query;
        }

        public Task<List<User>> GetAllAsync(
            params Expression<Func<User, object>>[] includes) =>
            WithIncludes(includes).ToListAsync();

        public Task<User?> GetByIdAsync(
            int id,
            params Expression<Func<User, object>>[] includes) =>
            WithIncludes(includes).FirstOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByEmailAsync(
            string email,
            params Expression<Func<User, object>>[] includes) =>
            WithIncludes(includes).FirstOrDefaultAsync(u => u.Email == email);

        public async Task AddAsync(User entity) =>
            await _context.Users.AddAsync(entity);

        public void Update(User entity) =>
            _context.Users.Update(entity);

        public void Delete(User entity) =>
            _context.Users.Remove(entity);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
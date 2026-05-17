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
          )
        {
            IQueryable<User> query = _context.Users;

            query = query.Include(u => u.Activity).Include(u => u.YouthCenter).Include(u => u.Reservations);
            return query;
        }

        public Task<List<User>> GetAllAsync(
            params Expression<Func<User, object>>[] includes) =>
            WithIncludes().ToListAsync();

        public Task<User?> GetByIdAsync(
            int id,
            params Expression<Func<User, object>>[] includes) =>
            WithIncludes().FirstOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByEmailAsync(
            string email,
            params Expression<Func<User, object>>[] includes) =>
            WithIncludes().FirstOrDefaultAsync(u => u.Email == email);

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
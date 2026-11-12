using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

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
        public Task<List<User>> SearchPlayersByEmailAsync(string term, int take) =>
            _context.Users.AsNoTracking()
                .Where(u => !u.IsDeleted && u.Role == UserRole.User && u.Email.StartsWith(term))
                .OrderBy(u => u.Email).Take(take).ToListAsync();
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
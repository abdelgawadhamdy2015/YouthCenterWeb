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
        public Task<User> AddAsync(User entity)
        {
            _context.Users.Add(entity);
            _context.SaveChanges();
            return Task.FromResult(entity);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var entity = _context.Users.Find(id);
            if (entity == null)
            {
                return Task.FromResult(false);
            }

            _context.Users.Remove(entity);
            _context.SaveChanges();
            return Task.FromResult(true);
        }

        public Task<List<User>> GetAllAsync(params Expression<Func<User, object>>[] includes)
        {
            IQueryable<User> query = _context.Users;

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query.ToListAsync();
        }

        public Task<User?> GetByEmailAsync(string email, params Expression<Func<User, object>>[] includes)
        {
            IQueryable<User> query = _context.Users.Where(u => u.Email == email);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query.FirstOrDefaultAsync();
        }


        public Task<User?> GetByIdAsync(int? id, params Expression<Func<User, object>>[] includes)
        {
            IQueryable<User> query = _context.Users.Where(u => u.Id == id);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query.FirstOrDefaultAsync();
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public Task<User> UpdateAsync(User entity)
        {
            _context.Users.Update(entity);
            _context.SaveChanges();
            return Task.FromResult(entity);
        }
    }
}
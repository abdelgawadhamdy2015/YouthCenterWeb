using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Data;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories
{
    public class YouthCenterRepo(AppDbContext context) : IYouthCenterRepo
    {
        private readonly AppDbContext _context = context;

        public Task<YouthCenter> CreateAsync(YouthCenter youthCenter)
        {


            _context.YouthCenters.Add(youthCenter);
            _context.SaveChanges();
            return Task.FromResult(youthCenter);

        }

        public async Task DeleteAsync(int id)
        {
            var youthCenter = await _context.YouthCenters.FindAsync(id);
            if (youthCenter != null)
            {
                _context.YouthCenters.Remove(youthCenter);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<YouthCenter>> GetAllAsync()
        {
            var query = _context.YouthCenters.AsQueryable();


            query = query.Include(y => y.YouthCenterActivities).ThenInclude(yca => yca.Activity);

            return await query.ToListAsync();
        }

        public async Task<YouthCenter?> GetByIdAsync(int id)
        {
            var youthCenter = await _context.YouthCenters.Include(y => y.YouthCenterActivities).ThenInclude(yca => yca.Activity).FirstOrDefaultAsync(y => y.Id == id);
            return youthCenter;
        }

        public async Task UpdateAsync(YouthCenter youthCenter)
        {
            _context.YouthCenters.Update(youthCenter);
            await _context.SaveChangesAsync();
        }
    }
}
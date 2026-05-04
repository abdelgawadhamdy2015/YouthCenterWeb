using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Data;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories
{
    public class YouthCenterActivityRepo(AppDbContext context) : IYouthCenterActivityRepo
    {
        private IQueryable<YouthCenterActivity> WithRelations() =>
           context.YouthCenterActivities
               .Include(y => y.YouthCenter)
            .Include(y => y.Activity);
        public async Task AddAsync(YouthCenterActivity entity) =>
            await context.YouthCenterActivities.AddAsync(entity);

        public async Task DeleteAsync(YouthCenterActivity entity) =>
            context.YouthCenterActivities.Remove(entity);

        public async Task<List<YouthCenterActivity>?> GetAllAsync(params Expression<Func<YouthCenterActivity, object>>[] includes) =>
            await WithRelations().ToListAsync();

        public async Task<List<YouthCenterActivity>?> GetByActivityId(int? activityId, params Expression<Func<YouthCenterActivity, object>>[] includes) =>
           await WithRelations().Where(yca => yca.ActivityId == activityId).ToListAsync();

        public async Task<YouthCenterActivity?> GetByIdAsync(int? id, params Expression<Func<YouthCenterActivity, object>>[] includes) =>
             context.YouthCenterActivities.FirstOrDefault(yca => yca.Id == id);

        public async Task<List<YouthCenterActivity>?> GetByYouthCenterId(int? youthCenterId, params Expression<Func<YouthCenterActivity, object>>[] includes) =>
            await WithRelations().Where(yca => yca.YouthCenterId == youthCenterId).ToListAsync();

        public async Task SaveChangesAsync() =>
           await context.SaveChangesAsync();

        public async Task UpdateAsync(YouthCenterActivity entity) =>
             context.YouthCenterActivities.Update(entity);

    }
}
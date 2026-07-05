using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories
{
    public class ActivityRepo(AppDbContext appDbContext) : IActivityRepo
    {
        private readonly AppDbContext context = appDbContext;
        private IQueryable<YouthCenterActivity> WithRelations() =>
             context.YouthCenterActivities
                .Include(x => x.Activity);

        public async Task<List<Activity>?> GetAllAsync(params Expression<Func<Activity, object>>[] includes)
        {
            return await WithRelations()
              .Select(x => x.Activity)
              .Distinct()
              .ToListAsync();
        }

        public async Task<List<Activity>> GetActivitiesByCenter(int youthCenterId)
        {
            return await context.YouthCenterActivities
                .Where(yca => yca.YouthCenterId == youthCenterId)
                .Select(yca => yca.Activity)
                .Distinct()
                .ToListAsync();
        }


        public async Task<Activity?> GetByIdAsync(int? id, params Expression<Func<Activity, object>>[] includes)
        {
            var yca = await WithRelations().FirstOrDefaultAsync(y => y.ActivityId == id);
            return yca?.Activity;
        }

        public async Task AddAsync(Activity entity) => await context.Activities.AddAsync(entity);


        public async Task DeleteAsync(Activity entity) => context.Activities.Remove(entity);
        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Activity entity)
        {
            context.Activities.Update(entity);
        }

        public async Task<List<Activity>> GetAvailableActivitiesForYouthCenter(int youthCenterId)
        {
            return await context.Activities
                .Where(a => !context.YouthCenterActivities
                    .Any(yca =>
                        yca.YouthCenterId == youthCenterId &&
                        yca.ActivityId == a.Id))
                .ToListAsync();
        }
    }
}
using System.Linq.Expressions;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class ActivityService(IActivityRepo activityRepo, IMapper<Activity, ActivityDto, CreateActivityDto> activityMapper) : IActivityService
    {
        private readonly IActivityRepo repo = activityRepo;
        private readonly IMapper<Activity, ActivityDto, CreateActivityDto> mapper = activityMapper;
        public async Task<ActivityDto> CreateAsync(CreateActivityDto dto)
        {
            var entity = mapper.CreateEntity(dto);
            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return mapper.ToDto(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return false;
            await repo.DeleteAsync(entity);
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<List<ActivityDto>> GetActivitiesByCenter(int youthCenterId)
        {
            var activities = await repo.GetActivitiesByCenter(youthCenterId);
            return activities.Select(mapper.ToDto).ToList();
        }

        public async Task<List<ActivityDto>?> GetAllAsync(params Expression<Func<Activity, object>>[] includes)
        {
            var activities = await repo.GetAllAsync();
            return activities?.Select(mapper.ToDto).ToList();
        }

        public async Task<ActivityDto?> GetByIdAsync(int id, params Expression<Func<Activity, object>>[] includes)
        {
            var activity = await repo.GetByIdAsync(id);
            if (activity == null) return null;
            return mapper.ToDto(activity);
        }
    }
}
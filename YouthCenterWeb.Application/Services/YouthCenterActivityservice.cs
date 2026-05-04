using System.Linq.Expressions;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class YouthCenterActivityService(IYouthCenterActivityRepo repo, IMapper<YouthCenterActivity, YouthCenterActivityDto, CreateYouthCenterActivityDto> mapper
) : IYouthCenterActivityService
    {
        public async Task<YouthCenterActivityDto> CreateAsync(CreateYouthCenterActivityDto dto)
        {
            var entity = mapper.CreateEntity(dto);
            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return mapper.ToDto(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var youthCenterActivity = await repo.GetByIdAsync(id)
                          ?? throw new NotFoundException("Reservation", id);

            await repo.DeleteAsync(youthCenterActivity);   // ← entity passed directly, no second DB call
            await repo.SaveChangesAsync();
            return true;
        }

        public async Task<List<YouthCenterActivityDto>?> GetAllAsync(params Expression<Func<YouthCenterActivity, object>>[] includes)
        {
            var ycas = await repo.GetAllAsync();
            return ycas?.Select(mapper.ToDto).ToList();
        }

        public async Task<YouthCenterActivityDto?> GetByIdAsync(int id, params Expression<Func<YouthCenterActivity, object>>[] includes)
        {
            var entity = await repo.GetByIdAsync(id);
            if (entity != null)
                return mapper.ToDto(entity);
            else return null;
        }

        public async Task<List<YouthCenterActivityDto>?> GetYouthCenterActivitiesByActivityIdAsync(int ActivityId)
        {
            var youthCenterActivities = await repo.GetByActivityId(ActivityId);
            return youthCenterActivities?.Select(mapper.ToDto).ToList();
        }

        public async Task<List<YouthCenterActivityDto>?> GetYouthCenterActivitiesByYouthCenterIdAsync(int YouthCenterId)
        {
            var youthCenterActivities = await repo.GetByYouthCenterId(YouthCenterId);
            return youthCenterActivities?.Select(mapper.ToDto).ToList();
        }
    }
}
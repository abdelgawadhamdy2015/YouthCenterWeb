using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mapper
{
    public class ActivityMapper : IMapper<Activity, ActivityDto>
    {
        public ActivityDto ToDto(Activity entity)
        {
            return new ActivityDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public Activity ToEntity(ActivityDto dto)
        {
            return new Activity
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }
    }
}
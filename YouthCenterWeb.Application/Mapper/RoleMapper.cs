using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mapper
{
    public class RoleMapper : IMapper<Role, RoleDto>
    {
        public RoleDto ToDto(Role entity)
        {
            return new RoleDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public Role ToEntity(RoleDto dto)
        {
            return new Role
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }
    }
}
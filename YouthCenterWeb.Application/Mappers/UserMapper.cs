using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;

public class UserMapper : IMapper<User, UserDto, CreateUserDto>
{
    public User CreateEntity(CreateUserDto createDto)
    {
        return new User
        {
            Name = createDto.Name,
            Email = createDto.Email,
            Mobile = createDto.Mobile,
            ImageUrl = createDto.ImageUrl,
            YouthCenterId = createDto.YouthCenterId,
            RoleId = createDto.RoleId
        };
    }

    public UserDto ToDto(User entity)
    {
        return new UserDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email,
            Mobile = entity.Mobile,
            ImageUrl = entity.ImageUrl,
            YouthCenterId = entity.YouthCenterId ?? 0,
            RoleId = entity.RoleId,
            ReservationCount = entity.ReservationCount

        };
    }

    public User ToEntity(UserDto dto)
    {
        return new User
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            Mobile = dto.Mobile,
            ImageUrl = dto.ImageUrl,
            YouthCenterId = dto.YouthCenterId,
            RoleId = dto.RoleId
        };
    }
}
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

public class RoleService(IGenericRepo<Role> genericRepo) : IRoleService
{

    private readonly IGenericRepo<Role> _genericRepo = genericRepo;
    public async Task<RoleDto> CreateAsync(RoleDto dto)
    {
        var role = new Role
        {
            Name = dto.Name
        };
        await _genericRepo.AddAsync(role);
        await _genericRepo.SaveChangesAsync();
        await _genericRepo.SaveChangesAsync();

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name
        };
    }


    public async Task<List<RoleDto>> GetAllAsync()
    {
        var roles = await _genericRepo.GetAllAsync();
        List<RoleDto> roleDtos = roles.Select(x => new RoleDto
        {
            Id = x.Id,
            Name = x.Name
        }
        ).ToList();
        return roleDtos;
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var role = await _genericRepo.GetByIdAsync(id);
        if (role == null) return null;
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name

        };
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _genericRepo.DeleteAsync(id);
        if (!result) return false;

        await _genericRepo.SaveChangesAsync();
        return true;
    }
}
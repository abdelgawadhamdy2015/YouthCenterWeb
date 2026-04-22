using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Models;

public interface IRoleService
{

    Task<List<RoleDto>> GetAllAsync();

    Task<RoleDto?> GetByIdAsync(int id);

    Task<RoleDto> CreateAsync(RoleDto dto);

    Task<bool> DeleteAsync(int id);

}
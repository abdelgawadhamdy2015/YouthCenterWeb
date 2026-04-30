using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IYouthCenterService
    {
        Task<YouthCenterDto?> GetByIdAsync(int id);
        Task<List<YouthCenterDto>> GetAllAsync();
        Task<YouthCenterDto> CreateAsync(CreateYouthCenterDto dto);
        Task<YouthCenterDto> UpdateAsync(int id, UpdatedYouthCenterDto dto);
        Task DeleteAsync(int id);
    }
}
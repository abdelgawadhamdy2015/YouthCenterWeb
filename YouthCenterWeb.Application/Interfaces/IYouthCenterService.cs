using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IYouthCenterService
    {
        Task<YouthCenterDto?> GetByIdAsync(int id);
        Task<List<YouthCenterDto>> GetAllAsync();
        Task<YouthCenterDto> CreateAsync(CreateYouthCenterDto dto);
        Task UpdateAsync(int id, YouthCenterDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
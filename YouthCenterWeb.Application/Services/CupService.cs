using System.Linq.Expressions;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services;

public class CupService(ICupRepo cupRepo, IMapper<Cup, CupDto, CreateCupDto> mapper) : ICupService
{
    public async Task<CupDto> CreateAsync(CreateCupDto dto)
    {
        var entity = mapper.CreateEntity(dto);
        await cupRepo.AddAsync(entity);
        await cupRepo.SaveChangesAsync();
        return mapper.ToDto(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await cupRepo.GetByIdAsync(id);
        if (entity == null) return false;
        await cupRepo.DeleteAsync(entity);
        await cupRepo.SaveChangesAsync();
        return true;

    }

    public async Task<List<CupDto>?> GetAllAsync(params Expression<Func<Cup, object>>[] includes)
    {
        var cups = await cupRepo.GetAllAsync();
        return cups?.Select(mapper.ToDto).ToList();
    }

    public async Task<CupDto?> GetByIdAsync(int id, params Expression<Func<Cup, object>>[] includes)
    {
        var cup = await cupRepo.GetByIdAsync(id);
        if (cup == null) return null;
        return mapper.ToDto(cup);
    }
}
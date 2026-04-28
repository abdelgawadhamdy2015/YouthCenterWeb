using AutoMapper;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class YouthCenterService(IYouthCenterRepo youthCenterRepo, IMapper<YouthCenter, YouthCenterDto, CreateYouthCenterDto> mapper) : IYouthCenterService
    {

        private readonly IYouthCenterRepo _youthCenterRepo = youthCenterRepo;
        private readonly IMapper<YouthCenter, YouthCenterDto, CreateYouthCenterDto> _mapper = mapper;
        public async Task<YouthCenterDto> CreateAsync(CreateYouthCenterDto dto)
        {

            var youthCenter = _mapper.CreateEntity(dto);

            var entity = await _youthCenterRepo.CreateAsync(youthCenter);

            return _mapper.ToDto(entity);

        }

        public async Task<bool> DeleteAsync(int id)
        {
            await _youthCenterRepo.DeleteAsync(id);
            return true;
        }

        public async Task<List<YouthCenterDto>> GetAllAsync()
        {
            var entities = await _youthCenterRepo.GetAllAsync();
            return entities.Select(_mapper.ToDto).ToList();
        }

        public async Task<YouthCenterDto?> GetByIdAsync(int id)
        {
            var entity = await _youthCenterRepo.GetByIdAsync(id);
            return entity == null ? null : _mapper.ToDto(entity);
        }

        public async Task UpdateAsync(int id, YouthCenterDto dto)
        {
            var youthCenter = _mapper.ToEntity(dto);
            await _youthCenterRepo.UpdateAsync(youthCenter);
        }
    }
}
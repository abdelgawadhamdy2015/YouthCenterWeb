using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class ReservationService(IReservationRepo repo, IMapper<Reservation, ReservationDto, CreateReservationDto> mapper)
    : IReservationService
    {
        private readonly IReservationRepo _repo = repo;
        private readonly IMapper<Reservation, ReservationDto, CreateReservationDto> _mapper = mapper;
        public async Task<List<ReservationDto>> GetAllAsync()
        {
            var data = await _repo.GetAllWithRelationsAsync();

            return data
                .Select(_mapper.ToDto)
                .ToList();
        }

        public async Task<List<ReservationDto>> GetUserReservationsAsync(int userId)
        {
            var reservations = await _repo.GetUserReservationsAsync(userId);

            return reservations
                .Select(_mapper.ToDto)
                .ToList();
        }
        public async Task<List<ReservationDto>> GetYouthCenterReservationsAsync(int youthCenterId)
        {
            var reservations = await _repo.GetYouthCenterReservationsAsync(youthCenterId);
            return reservations.Select(_mapper.ToDto).ToList();
        }
        public async Task<List<ReservationDto>> GetReservationsByStatusAsync(ReservationStatus reservationStatus)
        {
            var reservations = await _repo.GetReservationsByStatusAsync(reservationStatus);

            return reservations.Select(_mapper.ToDto).ToList();
        }
        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdWithRelationsAsync(id);

            return entity == null ? null : _mapper.ToDto(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var result = await _repo.DeleteAsync(id);
            if (!result) return false;

            await _repo.SaveChangesAsync();
            return true;
        }



        public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
        {

            var entity = _mapper.CreateEntity(dto);

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            return _mapper.ToDto(entity);
        }


    }
}
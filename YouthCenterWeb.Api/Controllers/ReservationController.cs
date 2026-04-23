using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationController(IReservationService service) : ControllerBase
    {
        private readonly IReservationService _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();

            return Ok(new BaseResponse<List<ReservationDto>>
            {
                Result = result.Count != 0 ? 1 : 0,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = result.Count != 0 ? Messages.Data.FoundAr : Messages.Data.NoDataAr,
                    MessageEn = result.Count != 0 ? Messages.Data.FoundEn : Messages.Data.NoDataEn
                }
            });
        }

        [HttpGet("UserId")]
        public async Task<IActionResult> GetUserReservations(int userId)
        {
            var result = await _service.GetUserReservationsAsync(userId);

            return Ok(new BaseResponse<List<ReservationDto>>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }

        [HttpGet("Status")]
        public async Task<IActionResult> GetReservationsByStatusAsync(ReservationStatus status)
        {
            var result = await _service.GetReservationsByStatusAsync(status);

            return Ok(new BaseResponse<List<ReservationDto>>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }
        [HttpGet("YouthCenterId")]
        public async Task<IActionResult> GetYouthCenterReservationsAsync(int youthCenterId)
        {
            var result = await _service.GetYouthCenterReservationsAsync(youthCenterId);

            return Ok(new BaseResponse<List<ReservationDto>>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            return Ok(new BaseResponse<ReservationDto>
            {
                Result = result == null ? 0 : 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = result == null ? Messages.Data.NoDataAr : Messages.Data.FoundAr,
                    MessageEn = result == null ? Messages.Data.NoDataEn : Messages.Data.FoundEn
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateReservationDto dto)
        {
            var result = await _service.CreateAsync(dto);

            return Ok(new BaseResponse<ReservationDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.CreatedAr,
                    MessageEn = Messages.Data.CreatedEn
                }
            });
        }
    }
}
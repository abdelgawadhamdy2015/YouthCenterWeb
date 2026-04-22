using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationController(IReservationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(new BaseResponse<List<ReservationDto>>
        {
            Result = 1,
            Data = result,
            Alert = new Alert
            {
                MessageAr = "تم العثور على الحجز",
                MessageEn = "Reservation found"
            }

        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
        {
            return NotFound(new BaseResponse<ReservationDto>
            {
                Result = 0,
                Alert = new Alert
                {
                    MessageAr = "الحجز غير موجود",
                    MessageEn = "Reservation not found"
                }
            });
        }

        return Ok(new BaseResponse<ReservationDto>
        {
            Result = 1,
            Data = result,
            Alert = new Alert
            {
                MessageAr = "تم العثور على الحجز",
                MessageEn = "Reservation found"
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
                MessageAr = "تم إنشاء الحجز",
                MessageEn = "Reservation created"
            }
        });
    }
}
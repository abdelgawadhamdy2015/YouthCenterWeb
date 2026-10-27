// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore.Metadata.Conventions;
// using YouthCenterWeb.Application.Common.Constants;
// using YouthCenterWeb.Application.Interfaces;
// using YouthCenterWeb.Data.DTOs;
// using YouthCenterWeb.DTOs;
// using YouthCenterWeb.Models;
// using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
// using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
// using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
// using YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions;
// using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

// namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize(Policy = Policies.RequireAuthenticated)]
//     public class ReservationController(IReservationService service) : ControllerBase
//     {
//         private readonly IReservationService _service = service;

//         // ── GET api/reservation ──────────────────────────────────────────────
//         // Admin   → sees only their center's reservations
//         // SuperAdmin → sees all
//         [HttpGet]
//         [Authorize(Policy = Policies.RequireAdmin)]
//         public async Task<IActionResult> GetAll()
//         {
//             var filter = new FilteredReservationDto();
//             var role = User.GetRole();

//             // Admin is scoped to their own center — SuperAdmin sees everything
//             if (role == UserRole.Admin)
//                 filter.YouthCenterId = User.GetYouthCenterId();

//             var result = await _service.GetReservationsAsync(filter);

//             return Ok(new BaseResponse<List<ReservationDto>>
//             {
//                 Result = result.Count != 0 ? 1 : 0,
//                 Data = result,
//                 Alert = new Alert
//                 {
//                     MessageAr = result.Count != 0 ? Messages.Data.FoundAr : Messages.Data.NoDataAr,
//                     MessageEn = result.Count != 0 ? Messages.Data.FoundEn : Messages.Data.NoDataEn
//                 }
//             });
//         }

//         // ── POST api/reservation/filters ─────────────────────────────────────
//         [HttpPost("filters")]
//         public async Task<IActionResult> GetFiltered(FilteredReservationDto dto)
//         {
//             var role = User.GetRole();

//             // Enforce scoping — ignore whatever the client sent for these fields
//             if (role == UserRole.User)
//             {
//                 dto.UserId = User.GetUserId();   // user only sees their own

//             }
//             else if (role == UserRole.Admin)
//             {
//                 dto.YouthCenterId = User.GetYouthCenterId(); // admin locked to own center
//             }
//             // SuperAdmin: dto passes through untouched — can filter anything

//             var result = await _service.GetReservationsAsync(dto);

//             return Ok(new BaseResponse<List<ReservationDto>>
//             {
//                 Result = result.Count != 0 ? 1 : 0,
//                 Data = result,
//                 Alert = new Alert
//                 {
//                     MessageAr = result.Count != 0 ? Messages.Data.FoundAr : Messages.Data.NoDataAr,
//                     MessageEn = result.Count != 0 ? Messages.Data.FoundEn : Messages.Data.NoDataEn
//                 }
//             });
//         }

//         // ── GET api/reservation/my ────────────────────────────────────────────
//         // — every user sees only their own
//         [HttpGet("my")]
//         public async Task<IActionResult> GetMy(int? pageNmper, int? pageSize)
//         {
//             var filter = new FilteredReservationDto
//             {
//                 UserId = User.GetUserId(),
//                 PageNumber = pageNmper,
//                 PageSize = pageSize
//             };

//             var result = await _service.GetReservationsAsync(filter);

//             return Ok(new BaseResponse<List<ReservationDto>>
//             {
//                 Result = result.Count != 0 ? 1 : 0,
//                 Data = result,
//                 Alert = new Alert
//                 {
//                     MessageAr = result.Count != 0 ? Messages.Data.FoundAr : Messages.Data.NoDataAr,
//                     MessageEn = result.Count != 0 ? Messages.Data.FoundEn : Messages.Data.NoDataEn
//                 }
//             });
//         }

//         // ── GET api/reservation/{id} ──────────────────────────────────────────
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetById(int id)
//         {
//             var result = await _service.GetByIdAsync(id)
//                 ?? throw new NotFoundException("Reservation", id);

//             return Ok(new BaseResponse<ReservationDto>
//             {
//                 Result = 1,
//                 Data = result,
//                 Alert = new Alert
//                 {
//                     MessageAr = Messages.Data.FoundAr,
//                     MessageEn = Messages.Data.FoundEn
//                 }
//             });
//         }

//         // ── POST api/reservation ─────────────────────────────────────────────
//         [HttpPost]
//         public async Task<IActionResult> Create(CreateReservationDto dto)
//         {
//             var result = await _service.CreateReservationAsync(
//                 dto,
//                 userId: User.GetUserId(),
//                 role: User.GetRole(),
//                 adminCenterId: User.GetYouthCenterId()
//             );

//             return CreatedAtAction(nameof(GetById), new { id = result?.Id },
//                 new BaseResponse<ReservationDto> { Result = 1, Data = result, Alert = new Alert { MessageAr = Messages.Data.CreatedAr, MessageEn = Messages.Data.CreatedEn } });
//         }

//         // ── UPDATE api/reservation/{id} ───────────────────────────────────────
//         [Authorize(policy: Policies.RequireAuthenticated)]
//         [HttpPost("update")]
//         public async Task<IActionResult> Update(UpdateReservationDto dto, int id)
//         {
//             var result = await _service.UpdateReservationAsync(
//                  dto,
//                  id,
//                 userId: User.GetUserId(),
//                 role: User.GetRole(),
//                 adminCenterId: User.GetYouthCenterId()
//             );
//             return Ok(new BaseResponse<ReservationDto>
//             {
//                 Result = 1,
//                 Data = result,
//                 Alert = new Alert
//                 {
//                     MessageAr = Messages.Data.UpdatedAr,
//                     MessageEn = Messages.Data.UpdatedEn
//                 }
//             });
//         }


//         // Get Avil Hours that user can choose it 
//         [Authorize(policy: Policies.RequireAuthenticated)]
//         [HttpPost("available-hours")]
//         public async Task<ActionResult> GetAvailHours([FromBody] AvailBookingDto dto)
//         {

//             List<PeakHourDto> hours = await _service.GetAvailHoursAsync(dto) ?? throw new NotFoundException(messageAr: Messages.Data.NoDataAr, messageEn: Messages.Data.NoDataEn);
//             if (hours.Count == 0)
//             {
//                 return NotFound(new BaseResponse<List<PeakHourDto>>
//                 {
//                     Result = 0,
//                     Alert = new Alert
//                     {
//                         MessageAr = Messages.Data.NoDataAr,
//                         MessageEn = Messages.Data.NoDataEn

//                     }
//                 });

//             }
//             else
//             {
//                 return Ok(new BaseResponse<List<PeakHourDto>>
//                 {
//                     Result = 1,
//                     Data = hours,
//                     Alert = new Alert
//                     {
//                         MessageAr = Messages.Data.NoDataAr,
//                         MessageEn = Messages.Data.NoDataEn

//                     }
//                 });
//             }
//         }


//         [HttpDelete("{id}")]
//         public async Task<IActionResult> Delete(int id)
//         {
//             await _service.DeleteAsync(
//                 id,
//                 userId: User.GetUserId(),
//                 role: User.GetRole(),
//                 adminCenterId: User.GetYouthCenterId()
//             );

//             return NoContent(); // 204 — standard for successful delete
//         }

//         // ── POST api/reservation/{id}/accept ─────────────────────────────────
//         [HttpPost("{id}/accept")]
//         [Authorize(Policy = Policies.RequireAdmin)]
//         public async Task<IActionResult> Accept(int id)
//         {
//             await _service.AcceptAsync(
//                 id,
//                 reviewedBy: User.GetUserId(),
//                 adminCenterId: User.GetYouthCenterId(),
//                 role: User.GetRole()
//             );



//             return Ok(new BaseResponse<string>
//             {
//                 Result = 1,
//                 Data = null,
//                 Alert = new Alert
//                 {
//                     MessageAr = Messages.Data.UpdatedAr,
//                     MessageEn = Messages.Data.UpdatedEn
//                 }
//             });
//         }

//         // ── POST api/reservation/{id}/reject ─────────────────────────────────
//         [HttpPost("{id}/reject")]
//         [Authorize(Policy = Policies.RequireAdmin)]
//         public async Task<IActionResult> Reject(int id, [FromBody] RejectReasonDto dto)
//         {
//             await _service.RejectAsync(
//                 id,
//                 reason: dto.Reason,
//                 reviewedBy: User.GetUserId(),
//                 adminCenterId: User.GetYouthCenterId(),
//                 role: User.GetRole()
//             );

//             return Ok(new BaseResponse<string>
//             {
//                 Result = 1,
//                 Data = null,
//                 Alert = new Alert
//                 {
//                     MessageAr = Messages.Data.UpdatedAr,
//                     MessageEn = Messages.Data.UpdatedEn
//                 }
//             });
//         }


//         [HttpPost("lock")]
//         public async Task<IActionResult> Lock(CreateReservationDto dto)
//         {
//             var result = await _service.LockForPaymentAsync(dto, User.GetUserId());
//             return Ok(new BaseResponse<ReservationDto>
//             {
//                 Result = 1,
//                 Data = result,
//                 Alert = new Alert { MessageAr = Messages.Data.CreatedAr, MessageEn = Messages.Data.CreatedEn }
//             });
//         }

//         // Called by YOUR backend after verifying the payment gateway's webhook — not directly by the app
//         [HttpPost("{id}/confirm-payment")]
//         public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentDto dto)
//         {

//             var result = await _service.ConfirmPaymentAsync(id, dto);
//             if (User.GetRole() == UserRole.User && result?.UserId != User.GetUserId())
//                 throw new NotFoundException("Reservation", id);
//             return Ok(new BaseResponse<ReservationDto> { Result = 1, Data = result });
//         }

//         [HttpPost("{id}/fail-payment")]
//         public async Task<IActionResult> FailPayment(int id, [FromBody] RejectReasonDto dto)
//         {
//             await _service.FailPaymentAsync(id, dto.Reason);
//             return Ok(new BaseResponse<string> { Result = 1 });
//         }

//         // ── POST api/reservation/{id}/pay-again ───────────────────────────────
//         [HttpPost("{id}/pay-again")]
//         public async Task<IActionResult> PayAgain(int id)
//         {
//             var result = await _service.PayAgainAsync(
//                 id,
//                 User.GetUserId());

//             return Ok(new BaseResponse<ReservationDto>
//             {
//                 Result = 1,
//                 Data = result,
//                 Alert = new Alert
//                 {
//                     MessageAr = "تم إنشاء رابط الدفع بنجاح.",
//                     MessageEn = "Payment link created successfully."
//                 }
//             });
//         }

//         [HttpPost("{id}/cancel")]
//         public async Task<IActionResult> Cancel(int id)
//         {
//             var result = await _service.CancelAsync(id, User.GetUserId());
//             return Ok(new BaseResponse<ReservationDto>
//             {
//                 Result = 1,
//                 Data = result,
//                 Alert = new Alert
//                 {
//                     MessageAr = "تم إلغاء الحجز.",
//                     MessageEn = "Reservation cancelled."
//                 }
//             });
//         }
//     }
// }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.RequireAuthenticated)]
    public class ReservationController(IReservationService service) : ControllerBase
    {
        private readonly IReservationService _service = service;

        private static BaseResponse<List<ReservationDto>> ListResponse(List<ReservationDto> result) =>
            new()
            {
                Result = result.Count != 0 ? 1 : 0,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = result.Count != 0 ? Messages.Data.FoundAr : Messages.Data.NoDataAr,
                    MessageEn = result.Count != 0 ? Messages.Data.FoundEn : Messages.Data.NoDataEn
                }
            };

        // ── GET api/reservation ──────────────────────────────────────────────
        [HttpGet]
        [Authorize(Policy = Policies.RequireAdmin)]
        public async Task<IActionResult> GetAll()
        {
            var filter = new FilteredReservationDto();

            if (User.GetRole() == UserRole.Admin)
                filter.YouthCenterId = User.GetYouthCenterId();

            return Ok(ListResponse(await _service.GetReservationsAsync(filter)));
        }

        // ── POST api/reservation/filters ─────────────────────────────────────
        [HttpPost("filters")]
        public async Task<IActionResult> GetFiltered(FilteredReservationDto dto)
        {
            var role = User.GetRole();

            if (role == UserRole.User)
                dto.UserId = User.GetUserId();
            else if (role == UserRole.Admin)
                dto.YouthCenterId = User.GetYouthCenterId();

            return Ok(ListResponse(await _service.GetReservationsAsync(dto)));
        }

        // ── GET api/reservation/my ───────────────────────────────────────────
        [HttpGet("my")]
        public async Task<IActionResult> GetMy(int? pageNmper, int? pageSize)
        {
            var filter = new FilteredReservationDto
            {
                UserId = User.GetUserId(),
                PageNumber = pageNmper,
                PageSize = pageSize
            };

            return Ok(ListResponse(await _service.GetReservationsAsync(filter)));
        }

        // ── GET api/reservation/{id} ─────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdSecuredAsync(
                id,
                userId: User.GetUserId(),
                role: User.GetRole(),
                adminCenterId: User.GetYouthCenterId())
                ?? throw new NotFoundException("Reservation", id);

            return Ok(new BaseResponse<ReservationDto>
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

        // ── POST api/reservation ─────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create(CreateReservationDto dto)
        {
            var result = await _service.CreateReservationAsync(
                dto,
                userId: User.GetUserId(),
                role: User.GetRole(),
                adminCenterId: User.GetYouthCenterId());

            return CreatedAtAction(nameof(GetById), new { id = result?.Id },
                new BaseResponse<ReservationDto>
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

        // ── POST api/reservation/update?id=.. ────────────────────────────────
        [HttpPost("update")]
        public async Task<IActionResult> Update(UpdateReservationDto dto, int id)
        {
            var result = await _service.UpdateReservationAsync(
                dto, id,
                userId: User.GetUserId(),
                role: User.GetRole(),
                adminCenterId: User.GetYouthCenterId());

            return Ok(new BaseResponse<ReservationDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.UpdatedAr,
                    MessageEn = Messages.Data.UpdatedEn
                }
            });
        }

        // ── POST api/reservation/available-hours ─────────────────────────────
        [HttpPost("available-hours")]
        public async Task<ActionResult> GetAvailHours([FromBody] AvailBookingDto dto)
        {
            var hours = await _service.GetAvailHoursAsync(dto)
                ?? throw new NotFoundException(
                    messageAr: Messages.Data.NoDataAr,
                    messageEn: Messages.Data.NoDataEn);

            if (hours.Count == 0)
            {
                return NotFound(new BaseResponse<List<PeakHourDto>>
                {
                    Result = 0,
                    Alert = new Alert
                    {
                        MessageAr = Messages.Data.NoDataAr,
                        MessageEn = Messages.Data.NoDataEn
                    }
                });
            }

            return Ok(new BaseResponse<List<PeakHourDto>>
            {
                Result = 1,
                Data = hours,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }

        // ── POST api/reservation/{id}/cancel ─────────────────────────────────
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _service.CancelAsync(id, User.GetUserId());

            return Ok(new BaseResponse<ReservationDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = "تم إلغاء الحجز.",
                    MessageEn = "Reservation cancelled."
                }
            });
        }

        // ── DELETE api/reservation/{id} ──────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(
                id,
                userId: User.GetUserId(),
                role: User.GetRole(),
                adminCenterId: User.GetYouthCenterId());

            return NoContent();
        }

        // ── POST api/reservation/{id}/accept ─────────────────────────────────
        [HttpPost("{id}/accept")]
        [Authorize(Policy = Policies.RequireAdmin)]
        public async Task<IActionResult> Accept(int id)
        {
            await _service.AcceptAsync(
                id,
                reviewedBy: User.GetUserId(),
                adminCenterId: User.GetYouthCenterId(),
                role: User.GetRole());

            return Ok(new BaseResponse<string>
            {
                Result = 1,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.UpdatedAr,
                    MessageEn = Messages.Data.UpdatedEn
                }
            });
        }

        // ── POST api/reservation/{id}/reject ─────────────────────────────────
        [HttpPost("{id}/reject")]
        [Authorize(Policy = Policies.RequireAdmin)]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectReasonDto dto)
        {
            await _service.RejectAsync(
                id,
                reason: dto.Reason,
                reviewedBy: User.GetUserId(),
                adminCenterId: User.GetYouthCenterId(),
                role: User.GetRole());

            return Ok(new BaseResponse<string>
            {
                Result = 1,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.UpdatedAr,
                    MessageEn = Messages.Data.UpdatedEn
                }
            });
        }

        // ── POST api/reservation/lock ────────────────────────────────────────
        [HttpPost("lock")]
        public async Task<IActionResult> Lock(CreateReservationDto dto)
        {
            var result = await _service.LockForPaymentAsync(dto, User.GetUserId());

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

        // ── تأكيد/فشل الدفع: للـ webhook الموثّق فقط، مش للاعبين ───────────────
        // (كانت مفتوحة لأي مستخدم مسجّل = أي لاعب يقدر يأكد حجزه بدون دفع)
        [HttpPost("{id}/confirm-payment")]
        [Authorize(Policy = Policies.RequireSuperAdmin)]
        public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentDto dto)
        {
            var result = await _service.ConfirmPaymentAsync(id, dto);
            return Ok(new BaseResponse<ReservationDto> { Result = 1, Data = result });
        }

        [HttpPost("{id}/fail-payment")]
        [Authorize(Policy = Policies.RequireSuperAdmin)]
        public async Task<IActionResult> FailPayment(int id, [FromBody] RejectReasonDto dto)
        {
            await _service.FailPaymentAsync(id, dto.Reason);
            return Ok(new BaseResponse<string> { Result = 1 });
        }

        // ── POST api/reservation/{id}/pay-again ──────────────────────────────
        [HttpPost("{id}/pay-again")]
        public async Task<IActionResult> PayAgain(int id)
        {
            var result = await _service.PayAgainAsync(id, User.GetUserId());

            return Ok(new BaseResponse<ReservationDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = "تم إنشاء رابط الدفع بنجاح.",
                    MessageEn = "Payment link created successfully."
                }
            });
        }
    }
}
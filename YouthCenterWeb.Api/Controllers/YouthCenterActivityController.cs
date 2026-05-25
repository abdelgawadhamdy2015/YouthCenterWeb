using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class YouthCenterActivityController(IYouthCenterActivityService service) : ControllerBase
    {
        private readonly IYouthCenterActivityService _youthCenterActivityService = service;

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the youth center ID from the current admin's JWT claims.
        /// Returns null if the claim is missing or invalid.
        /// </summary>
        private int? AdminYouthCenterId => User.GetYouthCenterId();

        /// <summary>
        /// Builds a standard 400 response for a missing / invalid youth center
        /// claim on the token — reused in every endpoint that needs it.
        /// </summary>
        private IActionResult MissingYouthCenterIdResponse() =>
            BadRequest(new BaseResponse<object>
            {
                Result = 0,
                Data = null,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.InvalidYouthCenterIdEn,
                    MessageAr = Messages.Data.InvalidYouthCenterIdAr,
                }
            });

        // ─────────────────────────────────────────────────────────────────────
        // GET  /api/YouthCenterActivity
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// SuperAdmin → returns all youth center activities.
        /// Admin      → returns only their own youth center's activities
        ///              (youthCenterId resolved from token, not from the request).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Admins are scoped to their own center automatically
            if (User.GetRole() == UserRole.Admin)
                return await GetByYouthCenterId();

            var result = await _youthCenterActivityService
                .GetAllAsync(y => y.Activity, y => y.YouthCenter);

            var found = result?.Count > 0;
            return Ok(new BaseResponse<List<YouthCenterActivityDto>>
            {
                Result = found ? 1 : 0,
                Data = result,
                DataCount = result?.Count ?? 0,
                Alert = new Alert
                {
                    MessageEn = found ? Messages.Data.FoundEn : Messages.Data.NoDataEn,
                    MessageAr = found ? Messages.Data.FoundAr : Messages.Data.NoDataAr,
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET  /api/YouthCenterActivity/{id}
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _youthCenterActivityService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new BaseResponse<YouthCenterActivityDto>
                {
                    Result = 0,
                    Data = null,
                    Alert = new Alert
                    {
                        MessageEn = Messages.Data.NoDataEn,
                        MessageAr = Messages.Data.NoDataAr,
                    }
                });

            return Ok(new BaseResponse<YouthCenterActivityDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.FoundEn,
                    MessageAr = Messages.Data.FoundAr,
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET  /api/YouthCenterActivity/activity/{activityId}
        // ─────────────────────────────────────────────────────────────────────

        [HttpGet("activity/{activityId}")]
        public async Task<IActionResult> GetByActivityId(int activityId)
        {

            var youthCenterId = User.GetYouthCenterId();
            if (User.GetRole() == UserRole.Admin)
            {
                if (youthCenterId == null)
                    return MissingYouthCenterIdResponse();
            }
            var result = await _youthCenterActivityService
                .GetYouthCenterActivitiesByActivityIdAsync(activityId, youthCenterId);

            if (result == null || result.Count == 0)
                return NotFound(new BaseResponse<List<YouthCenterActivityDto>>
                {
                    Result = 0,
                    Data = null,
                    Alert = new Alert
                    {
                        MessageEn = Messages.Data.NoDataEn,
                        MessageAr = Messages.Data.NoDataAr,
                    }
                });

            return Ok(new BaseResponse<List<YouthCenterActivityDto>>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.FoundEn,
                    MessageAr = Messages.Data.FoundAr,
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET  /api/YouthCenterActivity/youthCenter          ← admin (own center)
        // GET  /api/YouthCenterActivity/youthCenter/{id}     ← super-admin (any)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Admin calls this without an id — their center is resolved from the token.
        /// SuperAdmin can optionally pass a youthCenterId in the route.
        /// </summary>
        [HttpGet("youthCenter")]
        public Task<IActionResult> GetByYouthCenterId() =>
            GetByYouthCenterIdInternal(null);

        [HttpGet("youthCenter/{youthCenterId}")]
        public Task<IActionResult> GetByYouthCenterId(int youthCenterId) =>
            GetByYouthCenterIdInternal(youthCenterId);

        private async Task<IActionResult> GetByYouthCenterIdInternal(int? youthCenterId)
        {
            // Admin: always use the center bound to their account
            if (User.GetRole() == UserRole.Admin)
            {
                youthCenterId = AdminYouthCenterId;
                if (youthCenterId == null)
                    return MissingYouthCenterIdResponse();
            }

            var result = await _youthCenterActivityService
                .GetYouthCenterActivitiesByYouthCenterIdAsync(youthCenterId!.Value);

            if (result == null || result.Count == 0)
                return NotFound(new BaseResponse<List<YouthCenterActivityDto>>
                {
                    Result = 0,
                    Data = null,
                    Alert = new Alert
                    {
                        MessageEn = Messages.Data.NoDataEn,
                        MessageAr = Messages.Data.NoDataAr,
                    }
                });

            return Ok(new BaseResponse<List<YouthCenterActivityDto>>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.FoundEn,
                    MessageAr = Messages.Data.FoundAr,
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST  /api/YouthCenterActivity
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Admin: youth center is determined from the token, not the request body.
        /// SuperAdmin: must supply YouthCenterId in the dto.
        /// </summary>
        [Authorize(policy: Policies.RequireAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create(CreateYouthCenterActivityDto dto)
        {
            if (User.GetRole() == UserRole.Admin)
            {
                var centerId = AdminYouthCenterId;
                if (centerId == null)
                    return MissingYouthCenterIdResponse();

                // Overwrite whatever the client sent — admin is always scoped
                dto.YouthCenterId = centerId.Value;
            }

            var result = await _youthCenterActivityService.CreateAsync(dto);
            if (result == null)
                return BadRequest(new BaseResponse<YouthCenterActivityDto>
                {
                    Result = 0,
                    Data = null,
                    Alert = new Alert
                    {
                        MessageEn = Messages.Data.CreationFailedEn,
                        MessageAr = Messages.Data.CreationFailedAr,
                    }
                });

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                new BaseResponse<YouthCenterActivityDto>
                {
                    Result = 1,
                    Data = result,
                    Alert = new Alert
                    {
                        MessageEn = Messages.Data.CreatedEn,
                        MessageAr = Messages.Data.CreatedAr,
                    }
                });
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT  /api/YouthCenterActivity/{id}
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Admin: ownership is verified — they can only update activities that
        ///        belong to their own youth center.
        /// SuperAdmin: can update any activity.
        /// </summary>
        [Authorize(policy: Policies.RequireAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, YouthCenterActivityDto dto)
        {
            if (User.GetRole() == UserRole.Admin)
            {
                var centerId = AdminYouthCenterId;
                if (centerId == null)
                    return MissingYouthCenterIdResponse();

                // Prevent admin from moving an activity to a different center
                dto.YouthCenterId = centerId.Value;

                // Verify the record actually belongs to this admin's center
                var existing = await _youthCenterActivityService.GetByIdAsync(id);
                if (existing == null || existing.YouthCenterId != centerId.Value)
                    return Forbid();
            }

            var result = await _youthCenterActivityService.UpdateAsync(id, dto);
            if (result == null)
                return NotFound(new BaseResponse<YouthCenterActivityDto>
                {
                    Result = 0,
                    Data = null,
                    Alert = new Alert
                    {
                        MessageEn = Messages.Data.NoDataEn,
                        MessageAr = Messages.Data.NoDataAr,
                    }
                });

            return Ok(new BaseResponse<YouthCenterActivityDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.UpdatedEn,
                    MessageAr = Messages.Data.UpdatedAr,
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE  /api/YouthCenterActivity/{id}
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Admin: can only delete activities belonging to their own youth center.
        /// SuperAdmin: can delete any activity.
        /// </summary>

        [Authorize(policy: Policies.RequireAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (User.GetRole() == UserRole.Admin)
            {
                var centerId = AdminYouthCenterId;
                if (centerId == null)
                    return MissingYouthCenterIdResponse();

                // Verify ownership before deleting
                var existing = await _youthCenterActivityService.GetByIdAsync(id);
                if (existing == null || existing.YouthCenterId != centerId.Value)
                    return Forbid();
            }

            var result = await _youthCenterActivityService.DeleteAsync(id);
            if (!result)
                return NotFound(new BaseResponse<object>
                {
                    Result = 0,
                    Data = null,
                    Alert = new Alert
                    {
                        MessageEn = Messages.Data.DeletionFailedEn,
                        MessageAr = Messages.Data.DeletionFailedAr,
                    }
                });

            return Ok(new BaseResponse<object>
            {
                Result = 1,
                Data = null,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.DeletedEn,
                    MessageAr = Messages.Data.DeletedAr,
                }
            });
        }
    }
}
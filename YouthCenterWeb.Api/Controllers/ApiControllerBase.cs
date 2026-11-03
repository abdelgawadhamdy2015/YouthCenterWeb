using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected OkObjectResult OkResult<T>(T? data, string? ar = null, string? en = null) =>
        Ok(new BaseResponse<T>
        {
            Result = 1,
            Data = data,
            Alert = new Alert
            {
                MessageAr = ar ?? Messages.Data.FoundAr,
                MessageEn = en ?? Messages.Data.FoundEn
            }
        });

    protected OkObjectResult Deleted() =>
        Ok(new BaseResponse<object>
        {
            Result = 1,
            Alert = new Alert
            {
                MessageAr = Messages.Data.DeletedAr,
                MessageEn = Messages.Data.DeletedEn
            }
        });

    protected CreatedAtActionResult CreatedResult<T>(string action, object routeValues, T data) =>
        CreatedAtAction(action, routeValues, new BaseResponse<T>
        {
            Result = 1,
            Data = data,
            Alert = new Alert
            {
                MessageAr = Messages.Data.CreatedAr,
                MessageEn = Messages.Data.CreatedEn
            }
        });
}
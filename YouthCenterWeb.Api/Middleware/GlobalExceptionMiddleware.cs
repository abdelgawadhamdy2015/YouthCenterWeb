// API/Middleware/GlobalExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exceptions;

namespace YouthCenterWeb.YouthCenterWeb.API.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleAsync(context, ex);
        }
    }

    private static Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, ar, en) = ex switch
        {
            NotFoundException nfe =>
                (HttpStatusCode.NotFound, nfe.MessageAr, nfe.MessageEn),

            KeyNotFoundException =>
                (HttpStatusCode.NotFound,
                 ExceptionMessages.NotFound.Ar,
                 ExceptionMessages.NotFound.En),

            BusinessException be =>
                (HttpStatusCode.BadRequest, be.MessageAr, be.MessageEn),

            InvalidOperationException ioe =>
                (HttpStatusCode.BadRequest,
                 ExceptionMessages.BadRequest.Ar,
                 ioe.Message),          // service message in English, generic Arabic

            UnauthorizedAccessException =>
                (HttpStatusCode.Forbidden,
                 ExceptionMessages.Forbidden.Ar,
                 ExceptionMessages.Forbidden.En),

            _ =>
                (HttpStatusCode.InternalServerError,
                 ExceptionMessages.ServerError.Ar,
                 ExceptionMessages.ServerError.En)
        };

        var response = new BaseResponse<object>
        {
            Result = 0,
            Data = null,
            Alert = new Alert { MessageAr = ar, MessageEn = en }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.InterFaces;
using YouthCenterWeb.Services;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Application.Services;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;
using YouthCenterWeb.YouthCenterWeb.Application.Mappers;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure;

using YouthCenterWeb.Infrastructure.Data;
using YouthCenterWeb.Domain.Entities;
using System.Text.Json;
using global::YouthCenterWeb.YouthCenterWeb.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using global::YouthCenterWeb.YouthCenterWeb.Infrastructure.PushNotification;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

public static class DependencyInjection
{
    public static IServiceCollection AddIDependencyInjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.Configure<DatabaseOptions>(
           configuration.GetSection(DatabaseOptions.SectionName));

        services.AddDbContext<AppDbContext>((sp, options) =>
                {
                    var db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                    options.UseSqlServer(db.ConnectionString);
                });
        // services.AddDbContext<AppDbContext>(options =>
        //     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        // HttpContextAccessor
        services.AddHttpContextAccessor();

        // Payment Options
        services.Configure<PaymobOptions>(
                     configuration.GetSection(PaymobOptions.SectionName));
        // JWT Options
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));
        // JWT Options

        services.Configure<PaymobOptions>(
            configuration.GetSection(PaymobOptions.SectionName));

        // Mapper 
        services.AddScoped<IMapper<Activity, ActivityDto, CreateActivityDto>, ActivityMapper>();
        services.AddScoped<IMapper<Reservation, ReservationDto, CreateReservationDto>, ReservationMapper>();
        services.AddScoped<IMapper<User, UserDto, RegisterDto>, UserMapper>();
        services.AddScoped<IMapper<YouthCenter, YouthCenterDto, CreateYouthCenterDto>, YouthCenterMapper>();
        services.AddScoped<IMapper<YouthCenterActivity, YouthCenterActivityDto, CreateYouthCenterActivityDto>, YouthCenterActivityMapper>();

        services.AddAutoMapper(cfg => { }, typeof(CupProfile).Assembly);


        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        services.AddScoped<IPushNotificationSender, FirebasePushNotificationSender>();
        services.AddScoped<INotificationService, NotificationService>();

        // Repositories
        services.AddScoped<IAuthRepo, AuthRepo>();
        services.AddScoped<IReservationRepo, ReservationRepo>();
        services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
        services.AddScoped<IUserRepo, UserRepo>();
        services.AddScoped<IYouthCenterRepo, YouthCenterRepo>();
        services.AddScoped<IYouthCenterActivityRepo, YouthCenterActivityRepo>();
        services.AddScoped<IActivityRepo, ActivityRepo>();
        services.AddScoped<ICupRepo, CupRepo>();
        services.AddScoped<IAvailabilityRepo, AvailabilityRepo>();
        services.AddScoped<IOtpRepo, OtpRepo>();

        // services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IYouthCenterService, YouthCenterService>();
        services.AddScoped<IYouthCenterActivityService, YouthCenterActivityService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICupService, CupService>();
        services.AddScoped<IPaymentSignatureVerifier, PaymobSignatureVerifier>();
        services.AddHttpClient<IPaymentGateway, PaymobPaymentGateway>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHealthChecks()
            .AddCheck<FirebaseHealthCheck>("firebase");

        return services;
    }
}
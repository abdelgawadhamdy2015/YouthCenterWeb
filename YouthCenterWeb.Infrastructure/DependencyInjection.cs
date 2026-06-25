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
public static class DependencyInjection
{
    public static IServiceCollection AddIDependencyInjection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // HttpContextAccessor
        services.AddHttpContextAccessor();

        // Mapper 
        services.AddScoped<IMapper<Activity, ActivityDto, CreateActivityDto>, ActivityMapper>();
        services.AddScoped<IMapper<Reservation, ReservationDto, CreateReservationDto>, ReservationMapper>();
        services.AddScoped<IMapper<User, UserDto, RegisterDto>, UserMapper>();
        services.AddScoped<IMapper<YouthCenter, YouthCenterDto, CreateYouthCenterDto>, YouthCenterMapper>();
        services.AddScoped<IMapper<YouthCenterActivity, YouthCenterActivityDto, CreateYouthCenterActivityDto>, YouthCenterActivityMapper>();
        // Replace: services.AddAutoMapper(typeof(CupProfile));
        // With this:
        services.AddAutoMapper(cfg => { /* configuration if needed */ }, typeof(CupProfile).Assembly);
        // Repositories
        services.AddScoped<IAuthRepo, AuthRepo>();
        services.AddScoped<IReservationRepo, ReservationRepo>();
        services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
        services.AddScoped<IUserRepo, UserRepo>();
        services.AddScoped<IYouthCenterRepo, YouthCenterRepo>();
        services.AddScoped<IYouthCenterActivityRepo, YouthCenterActivityRepo>();
        services.AddScoped<IActivityRepo, ActivityRepo>();
        services.AddScoped<ICupRepo, CupRepo>();

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



        return services;
    }
}
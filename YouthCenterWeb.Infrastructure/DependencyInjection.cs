using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.InterFaces;
using YouthCenterWeb.Models;
using YouthCenterWeb.Services;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Extensions;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Application.Mapper;
using YouthCenterWeb.YouthCenterWeb.Application.Mappers;
using YouthCenterWeb.YouthCenterWeb.Application.Services;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure
{
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
            // Repositories
            services.AddScoped<IAuthRepo, AuthRepo>();
            services.AddScoped<IReservationRepo, ReservationRepo>();
            services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
            services.AddScoped<IUserRepo, UserRepo>();
            services.AddScoped<IYouthCenterRepo, YouthCenterRepo>();
            services.AddScoped<IYouthCenterActivityRepo, YouthCenterActivityRepo>();

            // services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddCrud<Activity, ActivityDto, CreateActivityDto>();
            services.AddCrud<YouthCenter, YouthCenterDto, CreateYouthCenterDto>();
            services.AddCrud<YouthCenterActivity, YouthCenterActivityDto, CreateYouthCenterActivityDto>();
            services.AddScoped<IYouthCenterService, YouthCenterService>();
            services.AddScoped<IYouthCenterActivityService, YouthCenterActivityService>();


            return services;
        }
    }
}
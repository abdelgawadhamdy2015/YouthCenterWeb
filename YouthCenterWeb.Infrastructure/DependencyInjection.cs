using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.InterFaces;
using YouthCenterWeb.Models;
using YouthCenterWeb.Services;
using YouthCenterWeb.YouthCenterWeb.Application.Extensions;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Application.Mapper;
using YouthCenterWeb.YouthCenterWeb.Application.Services;
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
            services.AddScoped<IMapper<Role, RoleDto, RoleDto>, RoleMapper>();
            services.AddScoped<IMapper<Activity, ActivityDto, CreateActivityDto>, ActivityMapper>();
            services.AddScoped<IMapper<Reservation, ReservationDto, CreateReservationDto>, ReservationMapper>();
            services.AddScoped<IMapper<Role, RoleDto, RoleDto>, RoleMapper>();
            services.AddScoped<IMapper<User, UserDto, CreateUserDto>, UserMapper>();

            // Repositories
            services.AddScoped<IAuthRepo, AuthRepo>();
            services.AddScoped<IReservationRepo, ReservationRepo>();
            services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
            services.AddScoped<IUserRepo, UserRepo>();
            // services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddCrud<Role, RoleDto, RoleDto>();
            services.AddCrud<Activity, ActivityDto, CreateActivityDto>();

            return services;
        }
    }
}
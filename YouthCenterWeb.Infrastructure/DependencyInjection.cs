using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Application.Services;
using YouthCenterWeb.Data;
using YouthCenterWeb.InterFaces;
using YouthCenterWeb.Services;
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

            // Repositories
            services.AddScoped<IAuthRepo, AuthRepo>();
            services.AddScoped<IReservationRepo, ReservationRepo>();
            services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
            // services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IRoleService, RoleService>();


            return services;
        }
    }
}
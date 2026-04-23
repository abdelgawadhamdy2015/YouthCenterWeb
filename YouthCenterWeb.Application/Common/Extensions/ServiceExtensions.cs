using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Application.Services;

namespace YouthCenterWeb.YouthCenterWeb.Application.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddCrud<TEntity, TDto, TCreateDto>(this IServiceCollection services)
            where TEntity : class
        {
            services.AddScoped<IGenericService<TEntity, TDto>, GenericService<TEntity, TDto, TCreateDto>>();

        }
    }
}
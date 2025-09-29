using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NutritionAPI.Infrastructure.Data;

namespace NutritionAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNutritionInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<NutritionDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("BrightnessDb")));

            return services;
        }
    }
}

using BrightNess.Library.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NutritionAPI.Infrastructure.Data;

namespace NutritionAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNutritionInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<NutritionDbContext>(services, configuration, configuration["MySerilog:FileName"] ?? "NutritionApi");

            return services;
        }
    }
}

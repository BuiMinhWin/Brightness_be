using BrightNess.Library.DI;
using MediaAPI.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediaInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<MediaDbContext>(services, configuration, configuration["MySerilog:FileName"] ?? "MediaApi");

            return services;
        }
    }
}

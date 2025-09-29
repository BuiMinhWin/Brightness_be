using MeasurementAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeasurementAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMeasurementInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MeasurementDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("BrightnessDb")));

            return services;
        }
    }
}

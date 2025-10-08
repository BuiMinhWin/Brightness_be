using MeasurementAPI.Application.Interfaces;
using MeasurementAPI.Application.Services;
using MeasurementAPI.Infrastructure.Data;
using MeasurementAPI.Infrastructure.Repositories;
using BrightNess.Library.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeasurementAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMeasurementInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<MeasurementDbContext>(services, configuration, configuration["MySerilog:FileName"] ?? "MeasurementApi");

            services.AddScoped<IBodyMeasurementRepository, BodyMeasurementRepository>();
            services.AddScoped<IMeasurementDeviceRepository, MeasurementDeviceRepository>();

            services.AddScoped<IBodyMeasurementService, BodyMeasurementService>();
            services.AddScoped<IMeasurementDeviceService, MeasurementDeviceService>();

            return services;
        }

        public static IApplicationBuilder UseMeasurementInfrastructure(this IApplicationBuilder app)
        {
            SharedServiceContainer.UseSharedPolicies(app);
            return app;
        }
    }
}

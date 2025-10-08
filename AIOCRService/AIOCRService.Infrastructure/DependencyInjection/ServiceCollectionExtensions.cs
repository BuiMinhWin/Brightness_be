using AIOCRService.Application.Interfaces;
using AIOCRService.Application.Services;
using AIOCRService.Infrastructure.Data;
using AIOCRService.Infrastructure.Repositories;
using AIOCRService.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIOCRService.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAIOCRInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<AIOCRDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("BrightnessDb")));

            // Repositories
            services.AddScoped<IOCRRepository, OCRRepository>();

            // Services
            services.AddScoped<IOCRService, OCRService>();

            // External services
            services.AddHttpClient<IExternalOCRService, GoogleVisionOCRService>();
            services.AddHttpClient<IExternalAIService, GoogleAIService>();

            return services;
        }

        public static IApplicationBuilder UseAIOCRInfrastructure(this IApplicationBuilder app)
        {
            return app;
        }
    }
}


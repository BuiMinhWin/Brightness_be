using BrightNess.Library.DI;
using MediaAPI.Application.Interfaces;
using MediaAPI.Application.Services;
using MediaAPI.Infrastructure.Data;
using MediaAPI.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediaAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediaInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<MediaDbContext>(services, configuration, configuration["MySerilog:FileName"] ?? "MediaApi");

            services.AddScoped<IMediaRepository, MediaRepository>();
            services.AddScoped<IMediaService, MediaService>();

            return services;
        }
    }
}

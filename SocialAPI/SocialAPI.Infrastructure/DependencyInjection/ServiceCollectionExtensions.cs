using BrightNess.Library.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialAPI.Infrastructure.Data;

namespace SocialAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSocialInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<SocialDbContext>(services, configuration, configuration["MySerilog:FileName"] ?? "SocialApi");

            return services;
        }
    }
}

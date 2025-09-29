using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialAPI.Infrastructure.Data;

namespace SocialAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSocialInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SocialDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("BrightnessDb")));

            return services;
        }
    }
}

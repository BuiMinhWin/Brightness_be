using ChallengeAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChallengeInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ChallengeDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("BrightnessDb")));

            return services;
        }
    }
}

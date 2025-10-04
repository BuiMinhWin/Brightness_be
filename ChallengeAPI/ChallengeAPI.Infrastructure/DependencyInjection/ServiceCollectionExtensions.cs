using BrightNess.Library.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ChallengeAPI.Infrastructure.Data;

namespace ChallengeAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChallengeInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<ChallengeDbContext>(services, configuration, configuration["MySerilog:FileName"] ?? "ChallengeApi");

            return services;
        }
    }
}

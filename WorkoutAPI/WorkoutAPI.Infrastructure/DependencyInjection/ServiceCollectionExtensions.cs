using BrightNess.Library.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkoutAPI.Infrastructure.Data;

namespace WorkoutAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkoutInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<WorkoutDbContext>(
                services,
                configuration,
                configuration["MySerilog:FileName"] ?? "WorkoutApi",
                schema: "workout");

            return services;
        }
    }
}

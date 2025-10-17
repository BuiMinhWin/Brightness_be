using BrightNess.Library.Configuration;
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Application.Services;
using ChallengeAPI.Application.ServicesImpl;
using ChallengeAPI.Infrastructure.Data;
using ChallengeAPI.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChallengeInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<ChallengeDbContext>(
                services,
                configuration,
                configuration["MySerilog:FileName"] ?? "ChallengeApi",
                schema: "challenge");

            services.AddScoped<IChallengeService, ChallengeService>();
            services.AddScoped<IChallengeRepository, ChallengeRepository>();

            services.AddScoped<IChallengeRuleService, ChallengeRuleService>();
            services.AddScoped<IChallengeRuleRepository, ChallengeRuleRepository>();

            services.AddScoped<IChallengeTaskService, ChallengeTaskService>();
            services.AddScoped<IChallengeTaskRepository, ChallengeTaskRepository>();

            services.AddScoped<IChallengeParticipantService, ChallengeParticipantService>();
            services.AddScoped<IChallengeParticipantRepository, ChallengeParticipantRepository>();

            services.AddScoped<IChallengeTaskProgressService, ChallengeTaskProgressService>();
            services.AddScoped<IChallengeTaskProgressRepository, ChallengeTaskProgressRepository>();

            services.AddScoped<IAchievementService, AchievementService>();
            services.AddScoped<IAchievementRepository, AchievementRepository>();

            services.AddScoped<ILeaderboardService, LeaderboardService>();

            return services;
        }
    }
}

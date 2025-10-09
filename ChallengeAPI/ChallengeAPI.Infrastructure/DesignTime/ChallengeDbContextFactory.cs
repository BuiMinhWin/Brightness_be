using System.IO;
using BrightNess.Library.Configuration;
using ChallengeAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChallengeAPI.Infrastructure.DesignTime
{
    public class ChallengeDbContextFactory : IDesignTimeDbContextFactory<ChallengeDbContext>
    {
        public ChallengeDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetRequiredConnectionString("BrightnessDb");

            var optionsBuilder = new DbContextOptionsBuilder<ChallengeDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ChallengeDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ChallengeAPI.Presentation");
            EnvironmentConfiguration.TryLoadDotEnv();
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}

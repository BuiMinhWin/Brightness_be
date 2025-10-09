using System.IO;
using BillingAPI.Infrastructure.Data;
using BrightNess.Library.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BillingAPI.Infrastructure.DesignTime
{
    public class BillingDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
    {
        public BillingDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetRequiredConnectionString("BrightnessDb");

            var optionsBuilder = new DbContextOptionsBuilder<BillingDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new BillingDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "BillingAPI.Presentation");
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

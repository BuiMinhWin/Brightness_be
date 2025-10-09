using System.IO;
using BrightNess.Library.Configuration;
using MeasurementAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MeasurementAPI.Infrastructure.DesignTime
{
    public class MeasurementDbContextFactory : IDesignTimeDbContextFactory<MeasurementDbContext>
    {
        public MeasurementDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetRequiredConnectionString("BrightnessDb");

            var optionsBuilder = new DbContextOptionsBuilder<MeasurementDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new MeasurementDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "MeasurementAPI.Presentation");
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

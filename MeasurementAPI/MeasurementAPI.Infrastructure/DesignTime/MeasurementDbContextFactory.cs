using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MeasurementAPI.Infrastructure.Data;

namespace MeasurementAPI.Infrastructure.DesignTime
{
    public class MeasurementDbContextFactory : IDesignTimeDbContextFactory<MeasurementDbContext>
    {
        public MeasurementDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetConnectionString("BrightnessDb")
                                   ?? throw new InvalidOperationException("Connection string 'BrightnessDb' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<MeasurementDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new MeasurementDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "MeasurementAPI.Presentation");
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}

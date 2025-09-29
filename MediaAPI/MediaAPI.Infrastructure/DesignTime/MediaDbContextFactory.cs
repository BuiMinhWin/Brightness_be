using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MediaAPI.Infrastructure.Data;

namespace MediaAPI.Infrastructure.DesignTime
{
    public class MediaDbContextFactory : IDesignTimeDbContextFactory<MediaDbContext>
    {
        public MediaDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetConnectionString("BrightnessDb")
                                   ?? throw new InvalidOperationException("Connection string 'BrightnessDb' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<MediaDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new MediaDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "MediaAPI.Presentation");
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}

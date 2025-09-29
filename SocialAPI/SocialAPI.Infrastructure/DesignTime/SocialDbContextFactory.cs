using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SocialAPI.Infrastructure.Data;

namespace SocialAPI.Infrastructure.DesignTime
{
    public class SocialDbContextFactory : IDesignTimeDbContextFactory<SocialDbContext>
    {
        public SocialDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetConnectionString("BrightnessDb")
                                   ?? throw new InvalidOperationException("Connection string 'BrightnessDb' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<SocialDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new SocialDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SocialAPI.Presentation");
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}

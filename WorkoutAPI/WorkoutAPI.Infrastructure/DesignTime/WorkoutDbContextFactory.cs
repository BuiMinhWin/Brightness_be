using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WorkoutAPI.Infrastructure.Data;

namespace WorkoutAPI.Infrastructure.DesignTime
{
    public class WorkoutDbContextFactory : IDesignTimeDbContextFactory<WorkoutDbContext>
    {
        public WorkoutDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetConnectionString("BrightnessDb")
                                   ?? throw new InvalidOperationException("Connection string 'BrightnessDb' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<WorkoutDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new WorkoutDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "WorkoutAPI.Presentation");
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}

using System.IO;
using BrightNess.Library.Configuration;
using ChatService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChatService.Infrastructure.DesignTime
{
    public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
    {
        public ChatDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var connectionString = configuration.GetRequiredConnectionString("BrightnessDb");

            var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ChatDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ChatService.Presentation");
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

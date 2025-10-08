using System;
using System.IO;
using AuthenticationAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthenticationAPI.Infrastructure.DesignTime
{
    public class AuthenticationDbContextFactory : IDesignTimeDbContextFactory<AuthenticationDbContext>
    {
        public AuthenticationDbContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "AuthenticationAPI.Presentation"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AuthenticationDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("BrightnessDb"));

            return new AuthenticationDbContext(optionsBuilder.Options);
        }
    }
}

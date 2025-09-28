using BrightNess.Library.Jwt;
using BrightNess.Library.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace BrightNess.Library.DI
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedServices<TContext>(this IServiceCollection services,
            IConfiguration config, string fileName) where TContext : DbContext
        {
            services.AddDbContext<TContext>(options =>
                options.UseNpgsql(
                    config.GetConnectionString("BrightnessDb"),
                    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: $"{fileName}-.text",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd:mm:ss.fff zzz} [{Level:u3}] {message: lj}{NewLine} {Exception}",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            services.Configure<JwtOptions>(config.GetSection("Authentication"));
            services.AddSingleton<ITokenService, TokenService>();

            JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, config);
            return services;
        }

        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalException>();
            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
            if (env.IsDevelopment())
            {
                app.UseWhen(
                    ctx => !ctx.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase),
                    ab => ab.UseMiddleware<ListenToOnlyAPIGateway>()
                );
            }
            else
            {
                app.UseMiddleware<ListenToOnlyAPIGateway>();
            }

            return app;
        }
    }
}

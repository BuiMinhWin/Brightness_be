using BrightNess.Library.Configuration;
using BrightNess.Library.Jwt;
using BrightNess.Library.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Serilog;
using System;
using System.Linq;

namespace BrightNess.Library.DI
{
    public static class SharedServiceContainer
    {
        private const string DefaultCorsPolicy = "DefaultCorsPolicy";

        public static IServiceCollection AddSharedServices<TContext>(this IServiceCollection services,
            IConfiguration config,
            string fileName,
            string? schema = null,
            Action<NpgsqlDbContextOptionsBuilder>? configureNpgsql = null) where TContext : DbContext
        {
            var connectionString = config.GetRequiredConnectionString("BrightnessDb");

            services.AddDbContext<TContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure();
                        if (!string.IsNullOrWhiteSpace(schema))
                        {
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema);
                        }
                        configureNpgsql?.Invoke(npgsqlOptions);
                    }));

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

            // JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, config);

            var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>();
            allowedOrigins = allowedOrigins?
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => origin.TrimEnd('/'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicy, builder =>
                {
                    if (allowedOrigins != null && allowedOrigins.Length > 0)
                    {
                        builder.WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                    else
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalException>();
            app.UseCors(DefaultCorsPolicy);

            app.UseWhen(
                ctx => !ctx.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase),
                ab => ab.UseMiddleware<ListenToOnlyAPIGateway>()
            );

            return app;
        }
    }
}





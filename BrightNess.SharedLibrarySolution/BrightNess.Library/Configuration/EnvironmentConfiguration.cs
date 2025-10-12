using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System;
using System.IO;

namespace BrightNess.Library.Configuration
{
    public static class EnvironmentConfiguration
    {
        private static bool _dotenvLoaded;

        public static void TryLoadDotEnv(IHostEnvironment? environment = null, string fileName = ".env")
        {
            if (_dotenvLoaded)
            {
                return;
            }

            if (environment != null && !environment.IsDevelopment())
            {
                return;
            }

            var envPath = FindEnvFile(fileName);
            if (string.IsNullOrEmpty(envPath))
            {
                return;
            }

            Env.Load(envPath);
            _dotenvLoaded = true;
        }

        public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
        {
            var connectionString = configuration.GetConnectionString(name);
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            connectionString = configuration[$"ConnectionStrings:{name}"];
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = GetRequired(configuration, "DB_HOST", name),
                Database = GetRequired(configuration, "DB_NAME", name),
                Username = GetRequired(configuration, "DB_USER", name),
                Password = GetRequired(configuration, "DB_PASSWORD", name),
                IncludeErrorDetail = GetBool(configuration, "DB_INCLUDE_ERROR_DETAIL") ?? true
            };

            if (int.TryParse(configuration["DB_PORT"], out var port))
            {
                builder.Port = port;
            }

            var sslMode = configuration["DB_SSLMODE"];
            if (!string.IsNullOrWhiteSpace(sslMode) && Enum.TryParse(sslMode, true, out SslMode parsedMode))
            {
                builder.SslMode = parsedMode;
            }

            return builder.ConnectionString;
        }

        private static string? FindEnvFile(string fileName)
        {
            var current = Directory.GetCurrentDirectory();

            while (!string.IsNullOrEmpty(current))
            {
                var candidate = Path.Combine(current, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = Directory.GetParent(current)?.FullName;
            }

            return null;
        }

        private static string GetRequired(IConfiguration configuration, string key, string connectionName)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Missing configuration '{key}' to build connection string '{connectionName}'.");
            }

            return value;
        }

        private static bool? GetBool(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return bool.TryParse(value, out var parsed) ? parsed : null;
        }
    }
}

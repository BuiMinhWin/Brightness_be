using BrightNess.Library.Configuration;
using ChallengeAPI.Infrastructure.Data;
using ChallengeAPI.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Collections.Generic;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
EnvironmentConfiguration.TryLoadDotEnv(builder.Environment);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter { DateTimeFormat = "dd-MM-yyyy" });
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Challenge API",
        Version = "v1"
    });
});
builder.Services.AddChallengeInfrastructure(builder.Configuration);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ChallengeDbContext>();
const int maxRetryAttempts = 15;
var delay = TimeSpan.FromSeconds(2);
var migrationSucceeded = false;
Exception? lastException = null;

for (var attempt = 1; attempt <= maxRetryAttempts; attempt++)
{
    try
    {
        await dbContext.Database.MigrateAsync();
        migrationSucceeded = true;
        break;
    }
    catch (Exception ex) when (IsDatabaseStartingUp(ex) && attempt < maxRetryAttempts)
    {
        lastException = ex;
        app.Logger.LogWarning(ex, "PostgreSQL is not ready yet (attempt {Attempt}/{MaxAttempts}). Waiting {Delay} before retry.", attempt, maxRetryAttempts, delay);
        await Task.Delay(delay);
    }
}

if (!migrationSucceeded)
{
    throw new InvalidOperationException($"Database migration failed because PostgreSQL was not ready after {maxRetryAttempts} attempts.", lastException);
}

var swaggerJsonEndpoint = builder.Configuration["Swagger:JsonEndpoint"] ?? "/swagger/v1/swagger.json";
var swaggerGatewayPrefix = builder.Configuration["Swagger:GatewayPrefix"]?.TrimEnd('/');

app.UseSwagger(cfg =>
{
    if (!string.IsNullOrWhiteSpace(swaggerGatewayPrefix))
    {
        cfg.PreSerializeFilters.Add((document, _) =>
        {
            document.Servers = new List<OpenApiServer>
            {
                new OpenApiServer { Url = swaggerGatewayPrefix }
            };
        });
    }
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint(swaggerJsonEndpoint, "Challenge API v1");
});

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

static bool IsDatabaseStartingUp(Exception ex)
{
    return ex switch
    {
        PostgresException postgres when postgres.SqlState == "57P03" => true,
        NpgsqlException npgsql when npgsql.SqlState == "57P03" => true,
        _ when ex.InnerException is not null => IsDatabaseStartingUp(ex.InnerException),
        _ => false
    };
}

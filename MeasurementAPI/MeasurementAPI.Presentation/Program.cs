using MeasurementAPI.Infrastructure.Data;
using MeasurementAPI.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Measurement API",
        Version = "v1",
        Description = "Collect body measurements, manage devices, and sync records"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityDefinition("ApiGatewayHeader", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Api-Gateway",
        Type = SecuritySchemeType.ApiKey,
        Description = "Set to any non-empty value when testing without API Gateway."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiGatewayHeader" }
            },
            Array.Empty<string>()
        }
    });

});

builder.Services.AddMeasurementInfrastructure(builder.Configuration);

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<MeasurementDbContext>();
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

app.UseMeasurementInfrastructure();

app.UseSwagger();
app.UseSwaggerUI();

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


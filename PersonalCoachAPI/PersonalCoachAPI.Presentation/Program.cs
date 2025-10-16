using BrightNess.Library.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Infrastructure.Data;
using PersonalCoachAPI.Infrastructure.Repositories;
using PersonalCoachAPI.Infrastructure.Services;
using PersonalCoachAPI.Presentation.Middleware;
using Pgvector.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSecretsManager();
EnvironmentConfiguration.TryLoadDotEnv(builder.Environment);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Personal Coach API", Version = "v1" });

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
        Description = "Set to any non-empty value when routing through the API Gateway."
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

// Add Database Context
var connectionString = builder.Configuration.GetRequiredConnectionString("DefaultConnection");
builder.Services.AddDbContext<PersonalCoachDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseVector();
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "personalcoach");
    });
});

var authSection = builder.Configuration.GetSection("Authentication");
var jwtKey = authSection["Key"] ?? throw new InvalidOperationException("Authentication:Key is not configured.");
var jwtIssuer = authSection["Issuer"] ?? throw new InvalidOperationException("Authentication:Issuer is not configured.");
var jwtAudience = authSection["Audience"] ?? throw new InvalidOperationException("Authentication:Audience is not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new { message = "You are not authorized" });
            return context.Response.WriteAsync(result);
        }
    };
});

// Add HTTP Client
builder.Services.AddHttpClient();

// Add Application Services
builder.Services.AddScoped<IRAGService, RAGService>();
builder.Services.AddScoped<IAIScheduleService, AIScheduleService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

// Add Repositories
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

// Add Database Seeder
builder.Services.AddScoped<DatabaseSeederService>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

const string CorsPolicyName = "ConfiguredCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins is { Length: > 0 })
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
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
    c.SwaggerEndpoint(swaggerJsonEndpoint, "Personal Coach API v1");
});

app.UseCors(CorsPolicyName);
app.UseHttpsRedirection();

// Internal API key validation (before authentication)
app.UseMiddleware<InternalApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Database migration on startup
await using var scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<PersonalCoachDbContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

const int maxRetryAttempts = 15;
var delay = TimeSpan.FromSeconds(2);
var migrationSucceeded = false;
Exception? lastException = null;

for (var attempt = 1; attempt <= maxRetryAttempts; attempt++)
{
    try
    {
        logger.LogInformation("Attempting database migration (attempt {Attempt}/{MaxAttempts})", attempt, maxRetryAttempts);
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migration completed successfully");
        migrationSucceeded = true;
        break;
    }
    catch (Exception ex) when (attempt < maxRetryAttempts)
    {
        lastException = ex;
        logger.LogWarning(ex, "PostgreSQL is not ready yet (attempt {Attempt}/{MaxAttempts}). Waiting {Delay} before retry.", attempt, maxRetryAttempts, delay);
        await Task.Delay(delay);
    }
}

if (!migrationSucceeded)
{
    logger.LogError(lastException, "Database migration failed after {MaxAttempts} attempts", maxRetryAttempts);
    throw new InvalidOperationException($"Database migration failed because PostgreSQL was not ready after {maxRetryAttempts} attempts.", lastException);
}

// Seed database after successful migration
try
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeederService>();
    await seeder.SeedAsync();
}
catch (Exception ex)
{
    logger.LogError(ex, "Error seeding database. Application will continue but database may be empty.");
}

app.Run();

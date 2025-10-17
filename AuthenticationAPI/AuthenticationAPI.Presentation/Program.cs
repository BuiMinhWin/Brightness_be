using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthenticationAPI.Infrastructure.Data;
using AuthenticationAPI.Infrastructure.DependencyInjection;
using BrightNess.Library.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSecretsManager();
EnvironmentConfiguration.TryLoadDotEnv(builder.Environment);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();

// CORS configuration for credentials
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCredentials", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for cookies
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Authentication API",
        Version = "v1",
        Description = "User authentication, registration, and JWT token management"
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
builder.Services.AddInfrastructureService(builder.Configuration);

var authSection = builder.Configuration.GetSection("Authentication");
var jwtKey = authSection["Key"] ?? throw new InvalidOperationException("Authentication:Key is not configured.");
var jwtIssuer = authSection["Issuer"] ?? throw new InvalidOperationException("Authentication:Issuer is not configured.");
var jwtAudience = authSection["Audience"] ?? throw new InvalidOperationException("Authentication:Audience is not configured.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
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

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
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

app.UserInfrastructurePolicy();
// Configure the HTTP request pipeline.
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
    c.SwaggerEndpoint(swaggerJsonEndpoint, "Authentication API V1");
});

app.UseCors("AllowCredentials");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Logger.LogInformation("--- AUTHENTICATION API: NEW VERSION DEPLOYED ---");
app.Run();

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

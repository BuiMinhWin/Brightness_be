using BrightNess.Library.Configuration;
using ChallengeAPI.Infrastructure.Data;
using ChallengeAPI.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Collections.Generic;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSecretsManager();
EnvironmentConfiguration.TryLoadDotEnv(builder.Environment);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
    //options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter { DateTimeFormat = "dd-MM-yyyy" });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Challenge API",
        Version = "v1"
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
builder.Services.AddChallengeInfrastructure(builder.Configuration);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        if (!jwtSettings.Exists() || string.IsNullOrWhiteSpace(jwtSettings["Key"]))
        {
            jwtSettings = builder.Configuration.GetSection("Authentication");
        }

        var signingKey = jwtSettings["Key"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured. Set Jwt:Key or Authentication:Key.");
        }

        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(signingKey))
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

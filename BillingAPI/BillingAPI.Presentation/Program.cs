using BillingAPI.Infrastructure.Data;
using BillingAPI.Infrastructure.DependencyInjection;
using BrightNess.Library.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
EnvironmentConfiguration.TryLoadDotEnv(builder.Environment);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Billing API",
        Version = "v1",
        Description = "Manage subscription plans, invoices, and payments"
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

builder.Services.AddBillingInfrastructure(builder.Configuration);

// Bind PaymentGateway options from configuration (appsettings, env vars, user-secrets, etc.)
builder.Services.Configure<BillingAPI.Presentation.PaymentGatewayOptions>(builder.Configuration.GetSection("PaymentGateway"));

// Validate / provide fallback for ApiKey and fail-fast in non-development.
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<BillingAPI.Presentation.PaymentGatewayOptions>>().Value;
    // First check bound value, then explicit env var fallback (supports PAYMENT_GATEWAY__APIKEY naming)
    if (string.IsNullOrWhiteSpace(cfg.ApiKey))
    {
        var fallback = builder.Configuration["PAYMENT_GATEWAY__APIKEY"] ?? builder.Configuration["PaymentGateway:ApiKey"];
        if (!string.IsNullOrWhiteSpace(fallback))
        {
            cfg.ApiKey = fallback;
        }
    }

    if (string.IsNullOrWhiteSpace(cfg.ApiKey) && !builder.Environment.IsDevelopment())
    {
        // Fail fast in production if no API key provided.
        throw new InvalidOperationException("PaymentGateway:ApiKey is not configured. Set PAYMENT_GATEWAY__APIKEY or configure a secrets provider for production.");
    }

    return cfg;
});

var app = builder.Build();

await using var scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
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

app.UseBillingInfrastructure();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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


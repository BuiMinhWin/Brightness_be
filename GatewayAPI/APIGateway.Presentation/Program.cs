using APIGateway.Presentation.Middleware;
using BrightNess.Library.Configuration;
using BrightNess.Library.DI;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddSecretsManager();
EnvironmentConfiguration.TryLoadDotEnv(builder.Environment);
builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console()
        .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
        .ReadFrom.Configuration(context.Configuration);
});

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Services.AddOcelot().AddCacheManager(x => x.WithDictionaryHandle());
// Configure the HTTP request pipeline.
JWTAuthenticationScheme.AddJWTAuthenticationScheme(builder.Services, builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        allowedOrigins = allowedOrigins?
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

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
var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<AttachSignature>();
app.UseOcelot().Wait();
app.Run();

using APIGateway.Presentation.Middleware;
using BrightNess.Library.Configuration;
using BrightNess.Library.DI;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System;
using System.Linq;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

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

// Add Swagger services for Gateway UI only (no Gateway API documentation)
// The Gateway UI will display Swagger docs from downstream services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

// Configure Swagger UI to display downstream service docs
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    // Add individual service Swagger endpoints (Gateway routes to services)
    c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "Authentication API");
    c.SwaggerEndpoint("/measurement/swagger/v1/swagger.json", "Measurement API");
    c.SwaggerEndpoint("/workout/swagger/v1/swagger.json", "Workout API");
    c.SwaggerEndpoint("/nutrition/swagger/v1/swagger.json", "Nutrition API");
    c.SwaggerEndpoint("/challenge/swagger/v1/swagger.json", "Challenge API");
    c.SwaggerEndpoint("/social/swagger/v1/swagger.json", "Social API");
    c.SwaggerEndpoint("/billing/swagger/v1/swagger.json", "Billing API");
    c.SwaggerEndpoint("/media/swagger/v1/swagger.json", "Media API");
    c.SwaggerEndpoint("/chat/swagger/v1/swagger.json", "Chat Service");
    c.SwaggerEndpoint("/ai-ocr/swagger/v1/swagger.json", "AI OCR Service");
    c.SwaggerEndpoint("/coach/swagger/v1/swagger.json", "Personal Coach API");
    
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Brightness API Gateway - All Services";
    c.DefaultModelsExpandDepth(-1);
    c.DisplayRequestDuration();
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<AttachSignature>();
app.UseOcelot().Wait();
app.Run();

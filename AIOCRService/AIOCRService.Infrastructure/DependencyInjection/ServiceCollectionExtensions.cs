using AIOCRService.Application.Interfaces;
using AIOCRService.Application.Services;
using AIOCRService.Infrastructure.Data;
using AIOCRService.Infrastructure.Repositories;
using AIOCRService.Infrastructure.Services;
using BrightNess.Library.Configuration;
using BrightNess.Library.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIOCRService.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAIOCRInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            SharedServiceContainer.AddSharedServices<AIOCRDbContext>(
                services,
                configuration,
                configuration["MySerilog:FileName"] ?? "AIOCRService",
                schema: "aiocr");

            // Repositories
            services.AddScoped<IOCRRepository, OCRRepository>();

            // Services
            services.AddScoped<IOCRService, OCRService>();
            services.AddScoped<IUserHealthDataService, UserHealthDataService>();
            
            // Credit Service (for token deduction)
            services.AddHttpClient<ICreditService, CreditService>();

            // External services - Use Gemini Vision for both OCR and AI processing
            services.AddHttpClient<IExternalOCRService, GoogleGeminiVisionService>();
            services.AddHttpClient<IExternalAIService, GoogleAIService>();
            
            // Optional: Keep old Google Vision as fallback
            // services.AddHttpClient<GoogleVisionOCRService>();

            return services;
        }

        public static IApplicationBuilder UseAIOCRInfrastructure(this IApplicationBuilder app)
        {
            return app;
        }
    }
}


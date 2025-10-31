using BrightNess.Library.DI;
using ChatService.Application.Interfaces;
using ChatService.Application.Services;
using ChatService.Infrastructure.Data;
using ChatService.Infrastructure.Repositories;
using ChatService.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChatInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<ChatDbContext>(
                services,
                configuration,
                configuration["MySerilog:FileName"] ?? "ChatService",
                schema: "chat");

            // Register repositories
            services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IConversationSummaryRepository, ConversationSummaryRepository>();

            // Register services
            services.AddHttpClient<IGeminiService, GeminiService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<IMessageLimitService, MessageLimitService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IAiUsageService, AiUsageService>();
            services.AddHttpClient<ICreditService, CreditService>();
            
            // Register security services
            services.AddScoped<IIntentDetectionService, IntentDetectionService>();
            services.AddScoped<IInputSanitizationService, InputSanitizationService>();
            services.AddScoped<IResponseValidationService, ResponseValidationService>();
            services.AddScoped<IUserContextCacheService, UserContextCacheService>();
            services.AddScoped<IProfileAnalysisService, ProfileAnalysisService>();
            
            // Register PersonalCoachAPI service with HttpClient
            services.AddHttpClient<IPersonalCoachApiService, PersonalCoachApiService>();
            
            // Add memory cache for context caching
            services.AddMemoryCache();

            return services;
        }
    }
}

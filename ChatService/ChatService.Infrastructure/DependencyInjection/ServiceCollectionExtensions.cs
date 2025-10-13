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
            SharedServiceContainer.AddSharedServices<ChatDbContext>(services, configuration, configuration["MySerilog:FileName"] ?? "ChatService");

            // Register repositories
            services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IConversationSummaryRepository, ConversationSummaryRepository>();

            // Register services
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IUserContextService, UserContextService>();
            services.AddScoped<IMessageLimitService, MessageLimitService>();
            services.AddScoped<IConversationService, ConversationService>();

            return services;
        }
    }
}

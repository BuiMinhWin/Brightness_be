using BrightNess.Library.DI;
using ChatService.Infrastructure.Data;
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

            return services;
        }
    }
}

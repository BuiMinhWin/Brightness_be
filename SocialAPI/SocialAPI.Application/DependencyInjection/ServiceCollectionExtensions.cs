using Microsoft.Extensions.DependencyInjection;
using SocialAPI.Application.Interfaces;
using SocialAPI.Application.Services;

namespace SocialAPI.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IReportService, ReportService>();
            return services;
        }
    }
}

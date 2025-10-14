using BrightNess.Library.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialAPI.Infrastructure.Data;
using SocialAPI.Application.Interfaces;
using SocialAPI.Infrastructure.Repositories;
using SocialAPI.Application.Services;


namespace SocialAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSocialInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<SocialDbContext>(
                services,
                configuration,
                configuration["MySerilog:FileName"] ?? "SocialApi",
                schema: "social");

            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IPostImageRepository, PostImageRepository>();
            services.AddScoped<IPostImageService, PostImageService>();

            return services;
        }
    }
}

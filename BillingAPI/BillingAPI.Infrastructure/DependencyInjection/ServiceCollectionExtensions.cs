using BillingAPI.Application.Interfaces;
using BillingAPI.Application.Services;
using BillingAPI.Infrastructure.Data;
using BillingAPI.Infrastructure.PaymentGateways;
using BillingAPI.Infrastructure.PaymentGateways.Options;
using BillingAPI.Infrastructure.Repositories;
using BrightNess.Library.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BillingAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBillingInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            SharedServiceContainer.AddSharedServices<BillingDbContext>(services, configuration, configuration["MySerilog:FileName"] ?? "BillingApi");

            services.Configure<PayOsOptions>(configuration.GetSection(PayOsOptions.SectionName));

            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPaymentService, PaymentService>();

            services.AddHttpClient<IPayOsGateway, PayOsGateway>();

            return services;
        }

        public static IApplicationBuilder UseBillingInfrastructure(this IApplicationBuilder app)
        {
            SharedServiceContainer.UseSharedPolicies(app);
            return app;
        }
    }
}

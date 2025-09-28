using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Infrastructure.Data;
using AuthenticationAPI.Infrastructure.Repositories;
using BrightNess.Library.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationAPI.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config
            )
        {
            //add database connectivity
            //JWT add authentication scheme
            SharedServiceContainer.AddSharedServices<AuthenticationDbContext>(services, config, config["MySerilog:FileName"]);

            //Create Dependency Injection
            services.AddScoped<IUser, UserRepository>();

            return services;
        }
        public static IApplicationBuilder UserInfrastructurePolicy(this IApplicationBuilder app) 
        {
            //Register middleware such as:
            //global exception: handle external errors
            //Listen only to API gateWay : block all outsiders call
            SharedServiceContainer.UseSharedPolicies(app);
            return app;
        }
        
    }
}

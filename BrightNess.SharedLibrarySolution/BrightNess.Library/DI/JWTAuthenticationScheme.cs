using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace BrightNess.Library.DI
{
    public static class JWTAuthenticationScheme
    {
        public static IServiceCollection AddJWTAuthenticationScheme(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var jwtSection = config.GetSection("Jwt");
                    if (!jwtSection.Exists() || string.IsNullOrWhiteSpace(jwtSection["Key"]))
                    {
                        jwtSection = config.GetSection("Authentication");
                    }

                    var keyValue = jwtSection["Key"];
                    var issuer = jwtSection["Issuer"];
                    var audience = jwtSection["Audience"];

                    if (string.IsNullOrWhiteSpace(keyValue))
                    {
                        throw new InvalidOperationException("JWT signing key is not configured. Set Jwt:Key or Authentication:Key.");
                    }

                    if (string.IsNullOrWhiteSpace(issuer))
                    {
                        throw new InvalidOperationException("JWT issuer is not configured. Set Jwt:Issuer or Authentication:Issuer.");
                    }

                    if (string.IsNullOrWhiteSpace(audience))
                    {
                        throw new InvalidOperationException("JWT audience is not configured. Set Jwt:Audience or Authentication:Audience.");
                    }

                    var key = Encoding.UTF8.GetBytes(keyValue);

                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.FromMinutes(2)
                    };
                });

            return services;
        }
    }
}

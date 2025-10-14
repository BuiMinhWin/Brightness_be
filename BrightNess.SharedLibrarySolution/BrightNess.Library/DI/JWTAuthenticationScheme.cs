using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightNess.Library.Security;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

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
                    var requireHttpsMetadata = jwtSection.GetValue<bool>("RequireHttpsMetadata", false);
                    var clockSkewMinutes = jwtSection.GetValue<int>("ClockSkewMinutes", 2);

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

                    options.RequireHttpsMetadata = requireHttpsMetadata;
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
                        ClockSkew = TimeSpan.FromMinutes(clockSkewMinutes)
                    };

                    // Enhanced token validation with blacklist checking
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var blacklistService = context.HttpContext.RequestServices.GetService<IJwtBlacklistService>();
                            if (blacklistService != null)
                            {
                                var jwtToken = context.SecurityToken as JwtSecurityToken;
                                var jti = jwtToken?.Claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                                var userId = jwtToken?.Claims?.FirstOrDefault(x => x.Type == "uid" || x.Type == "UserId")?.Value;

                                if (!string.IsNullOrEmpty(jti) && await blacklistService.IsTokenBlacklistedAsync(jti))
                                {
                                    var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerHandler>>();
                                    logger?.LogWarning("Blacklisted token detected: {JTI}", jti);
                                    context.Fail("Token has been revoked");
                                    return;
                                }

                                // Check if all user tokens are blacklisted
                                if (!string.IsNullOrEmpty(userId))
                                {
                                    var isUserBlacklisted = await blacklistService.IsTokenBlacklistedAsync($"user:{userId}");
                                    if (isUserBlacklisted)
                                    {
                                        var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerHandler>>();
                                        logger?.LogWarning("User tokens blacklisted: {UserId}", userId);
                                        context.Fail("User session has been terminated");
                                        return;
                                    }
                                }
                            }
                        },
                        
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            var result = System.Text.Json.JsonSerializer.Serialize(new { 
                                message = "You are not authorized",
                                timestamp = DateTime.UtcNow,
                                path = context.Request.Path.Value
                            });
                            await context.Response.WriteAsync(result);
                        },

                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<JwtBearerHandler>>();
                            logger?.LogWarning("JWT Authentication failed: {Exception}", context.Exception.Message);
                            
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers["Token-Expired"] = "true";
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}

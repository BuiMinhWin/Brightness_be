using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationAPI.Application.DTOs;
using AuthenticationAPI.Application.Interfaces;
using BrightNess.Library.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AuthenticationAPI.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserService userService, IConfiguration configuration) : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<Response>> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await userService.RegisterAsync(registerDTO);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<Response>> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ipAddress = GetClientIpAddress();
            var result = await userService.LoginAsync(loginDTO, ipAddress);
            
            if (result.Flag && result.Data is AuthTokensDTO tokens)
            {
                // Set httpOnly cookie with refresh token
                SetRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
                
                // Return only access token in response body (refresh token in cookie)
                var responseData = new
                {
                    accessToken = tokens.AccessToken,
                    accessTokenExpiresAt = tokens.AccessTokenExpiresAt,
                    refreshToken = tokens.RefreshToken, // Still include for backward compatibility
                    refreshTokenExpiresAt = tokens.RefreshTokenExpiresAt
                };
                
                return Ok(new Response(true, result.Message, responseData));
            }
            
            return BadRequest(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<Response>> RefreshToken([FromBody] RefreshTokenRequest? request = null)
        {
            // Get refresh token from cookie (preferred) or body (fallback)
            var refreshToken = GetRefreshTokenFromRequest(request);
            
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new Response(false, "Refresh token is required"));
            }
            
            var result = await userService.RefreshTokenAsync(refreshToken, GetClientIpAddress());
            
            if (result.Flag && result.Data is AuthTokensDTO tokens)
            {
                // Set new httpOnly cookie with new refresh token
                SetRefreshTokenCookie(tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
                
                // Return only access token in response body
                var responseData = new
                {
                    accessToken = tokens.AccessToken,
                    accessTokenExpiresAt = tokens.AccessTokenExpiresAt,
                    refreshToken = tokens.RefreshToken, // Still include for backward compatibility
                    refreshTokenExpiresAt = tokens.RefreshTokenExpiresAt
                };
                
                return Ok(new Response(true, result.Message, responseData));
            }
            
            return BadRequest(result);
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<ActionResult<Response>> RevokeToken([FromBody] RefreshTokenRequest? request = null)
        {
            // Get refresh token from cookie (preferred) or body (fallback)
            var refreshToken = GetRefreshTokenFromRequest(request);
            
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new Response(false, "Refresh token is required"));
            }
            
            var result = await userService.RevokeRefreshTokenAsync(refreshToken, GetClientIpAddress());
            
            if (result.Flag)
            {
                // Clear the httpOnly cookie
                ClearRefreshTokenCookie();
            }
            
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous] // Allow internal service-to-service communication
        public async Task<ActionResult<GetUserDTO>> GetUser(Guid id)
        {
            var user = await userService.GetByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GetUserDTO>>> GetUsers()
        {
            var users = await userService.GetAllAsync();
            var userList = users?.ToList();

            if (userList is null || userList.Count == 0)
            {
                return NotFound();
            }

            return Ok(userList);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<Response>> Update(Guid id, UpdateUserDTO updateUserDto)
        {
            var result = await userService.UpdateAsync(id, updateUserDto);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id:guid}/subscription")]
        [AllowAnonymous] // Allow BillingAPI to call without user token
        public async Task<ActionResult<Response>> UpdateSubscription(Guid id, [FromBody] UpdateSubscriptionRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await userService.UpdateSubscriptionAsync(id, request.SubscriptionPlanId);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<Response>> Delete(Guid id)
        {
            var result = await userService.DeleteAsync(id);
            return result.Flag ? Ok(result) : NotFound(result);
        }

        private string? GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// Sets the refresh token as an httpOnly cookie
        /// </summary>
        private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
        {
            var cookieName = _configuration["CookieSettings:RefreshTokenCookieName"] ?? "refreshToken";
            var isDevelopment = HttpContext.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() ?? false;
            var isSecure = _configuration.GetValue<bool>("CookieSettings:SecureInProduction");
            var sameSite = _configuration["CookieSettings:SameSite"] ?? "Lax";

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Cannot be accessed by JavaScript
                Secure = isDevelopment ? false : isSecure, // Only sent over HTTPS in production
                SameSite = sameSite switch
                {
                    "Strict" => SameSiteMode.Strict,
                    "None" => SameSiteMode.None,
                    _ => SameSiteMode.Lax
                },
                Expires = expiresAt,
                Path = "/auth/api/Authentication", // Scoped to auth endpoints
                IsEssential = true // Not affected by cookie consent
            };

            Response.Cookies.Append(cookieName, refreshToken, cookieOptions);
        }

        /// <summary>
        /// Clears the refresh token cookie
        /// </summary>
        private void ClearRefreshTokenCookie()
        {
            var cookieName = _configuration["CookieSettings:RefreshTokenCookieName"] ?? "refreshToken";
            
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/auth/api/Authentication",
                Expires = DateTime.UtcNow.AddDays(-1) // Expire in the past
            };

            Response.Cookies.Delete(cookieName, cookieOptions);
        }

        /// <summary>
        /// Gets refresh token from cookie (primary) or request body (fallback for backward compatibility)
        /// </summary>
        private string? GetRefreshTokenFromRequest(RefreshTokenRequest? request = null)
        {
            var cookieName = _configuration["CookieSettings:RefreshTokenCookieName"] ?? "refreshToken";
            
            // Try cookie first (preferred method)
            if (Request.Cookies.TryGetValue(cookieName, out var cookieToken) && !string.IsNullOrWhiteSpace(cookieToken))
            {
                return cookieToken;
            }

            // Fallback to body for backward compatibility
            return request?.RefreshToken;
        }
    }
}


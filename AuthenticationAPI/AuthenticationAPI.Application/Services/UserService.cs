using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AuthenticationAPI.Application.DTOs;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;
using BrightNess.Library.Jwt;
using BrightNess.Library.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace AuthenticationAPI.Application.Services
{
    public class UserService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenCacheService refreshTokenCacheService,
        IUserExternalLoginRepository userExternalLoginRepository,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IAvatarStorageService avatarStorageService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        private readonly IRefreshTokenCacheService _refreshTokenCache = refreshTokenCacheService ?? throw new ArgumentNullException(nameof(refreshTokenCacheService));
        private readonly IUserExternalLoginRepository _externalLoginRepository = userExternalLoginRepository ?? throw new ArgumentNullException(nameof(userExternalLoginRepository));
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        private readonly IAvatarStorageService _avatarStorageService = avatarStorageService ?? throw new ArgumentNullException(nameof(avatarStorageService));

        public async Task<Response> RegisterAsync(RegisterDTO userDTO)
        {
            var existingByEmail = await _userRepository.GetByEmailAsync(userDTO.Email);
            if (existingByEmail is not null)
            {
                return new Response(false, "Email already registered");
            }

            var usernameExists = await _userRepository.UserNameExistsAsync(userDTO.UserName);
            if (usernameExists)
            {
                return new Response(false, "Username already taken");
            }

            var userId = userDTO.Id ?? Guid.NewGuid();
            var profilePictureUrl = await ProcessIncomingProfilePictureAsync(userId, userDTO.ProfilePicture);

            var user = new User
            {
                Id = userId,
                Email = userDTO.Email.Trim(),
                UserName = userDTO.UserName.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                FullName = userDTO.FullName,
                PhoneNumber = userDTO.PhoneNumber,
                Gender = userDTO.Gender,
                ProfilePicture = profilePictureUrl,
                DateOfBirth = userDTO.DateOfBirth,
                Role = string.IsNullOrWhiteSpace(userDTO.Role) ? "user" : userDTO.Role,
                Status = string.IsNullOrWhiteSpace(userDTO.Status) ? "active" : userDTO.Status,
                SubscriptionPlanId = userDTO.SubscriptionPlanId,
                PaymentCustomerRef = userDTO.PaymentCustomerRef,
                CreatedAt = DateTime.UtcNow
            };

            var createResponse = await _userRepository.CreateAsync(user);
            return createResponse.Flag
                ? new Response(true, "Register successfully")
                : createResponse;
        }

        public async Task<Response> LoginAsync(LoginDTO loginDTO, string? ipAddress = null)
        {
            var user = await _userRepository.GetByEmailAsync(loginDTO.email);
            if (user is null)
            {
                return new Response(false, "Invalid credentials");
            }

            var passwordMatches = BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash);
            if (!passwordMatches)
            {
                return new Response(false, "Invalid credentials");
            }

            return await GenerateLoginResponseAsync(user, loginDTO.RememberMe, ipAddress);
        }

        public async Task<Response> ExternalLoginAsync(ExternalLoginDTO externalAuth, string? ipAddress = null)
        {
            var validationResult = await ValidateExternalToken(externalAuth.Provider, externalAuth.Token);
            if (!validationResult.IsValid)
            {
                return new Response(false, validationResult.Error ?? "External authentication failed.");
            }

            if (string.IsNullOrWhiteSpace(validationResult.Email))
            {
                return new Response(false, "Email information is required for external login.");
            }

            User? user = null;

            if (!string.IsNullOrWhiteSpace(validationResult.ProviderKey))
            {
                var linkedLogin = await _externalLoginRepository.GetByProviderAsync(externalAuth.Provider, validationResult.ProviderKey);
                user = linkedLogin?.User;
            }

            user ??= await EnsureUserForExternalLoginAsync(
                externalAuth.Provider,
                validationResult.Email,
                validationResult.Name,
                validationResult.Picture,
                validationResult.ProviderKey);

            if (user is null)
            {
                return new Response(false, "Could not create or find user.");
            }

            return await GenerateLoginResponseAsync(user, rememberMe: false, ipAddress);
        }

        private async Task<(bool IsValid, string? Error, string? Email, string? Name, string? Picture, string? ProviderKey)> ValidateExternalToken(string provider, string token)
        {
            var client = _httpClientFactory.CreateClient();
            try
            {
                JObject? userObject = null;
                if (provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
                {
                    var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={token}");
                    if (!response.IsSuccessStatusCode)
                        return (false, "Invalid Google token", null, null, null, null);
                    userObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    return (false, "Unsupported provider", null, null, null, null);
                }

                if (userObject is null)
                {
                    return (false, "Unable to read external provider response", null, null, null, null);
                }

                var email = userObject["email"]?.ToString();
                if (string.IsNullOrWhiteSpace(email))
                {
                    return (false, "Email not provided by external provider", null, null, null, null);
                }

                var name = userObject["name"]?.ToString();
                var picture = userObject["picture"]?.ToString();
                var providerKey = userObject["sub"]?.ToString();

                return (true, null, email, name, picture, providerKey);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null, null, null, null);
            }
        }

        private async Task<User?> EnsureUserForExternalLoginAsync(string provider, string email, string? name, string? pictureUrl, string? providerKey)
        {
            var trimmedEmail = email.Trim();
            var user = await _userRepository.GetByEmailAsync(trimmedEmail);

            if (user is null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = trimmedEmail,
                    UserName = trimmedEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                    FullName = string.IsNullOrWhiteSpace(name) ? trimmedEmail : name,
                    ProfilePicture = pictureUrl,
                    Role = "user",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow
                };

                var createUserResponse = await _userRepository.CreateAsync(user);
                if (!createUserResponse.Flag)
                {
                    return null;
                }
            }

            await EnsureExternalLoginLinkAsync(user, provider, providerKey, trimmedEmail);

            return user;
        }

        private async Task EnsureExternalLoginLinkAsync(User user, string provider, string? providerKey, string email)
        {
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(providerKey))
            {
                return;
            }

            var existingLink = await _externalLoginRepository.GetByProviderAsync(provider, providerKey);
            if (existingLink is not null)
            {
                return;
            }

            existingLink = await _externalLoginRepository.GetByUserAsync(provider, user.Id);
            if (existingLink is not null)
            {
                return;
            }

            var externalLogin = new UserExternalLogin
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Provider = provider.Trim(),
                ProviderKey = providerKey.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                CreatedAt = DateTime.UtcNow
            };

            await _externalLoginRepository.CreateAsync(externalLogin);
        }

        private async Task<Response> GenerateLoginResponseAsync(User user, bool rememberMe, string? ipAddress)
        {
            var payload = new TokenPayload
            {
                Id = user.Id.ToString(),
                Name = user.FullName ?? user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            var refreshTokenDays = rememberMe
                ? _configuration.GetValue<int>("Authentication:RefreshTokenExpirationDaysExtended")
                : _configuration.GetValue<int>("Authentication:RefreshTokenExpirationDays");

            var tokens = _tokenService.GenerateAuthTokens(payload, refreshTokenDays);

            var revokeExistingResult = await _refreshTokenRepository.RevokeActiveTokensAsync(user.Id, tokens.RefreshToken, ipAddress);
            if (!revokeExistingResult.Flag)
            {
                return new Response(false, "Unable to revoke existing sessions", revokeExistingResult.Message);
            }

            await _refreshTokenCache.RemoveTokensByUserAsync(user.Id);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = tokens.RefreshToken,
                ExpiresAt = tokens.RefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            var saveTokenResult = await _refreshTokenRepository.CreateAsync(refreshToken);
            if (!saveTokenResult.Flag)
            {
                return new Response(false, "Unable to persist refresh token", saveTokenResult.Message);
            }

            await _refreshTokenCache.CacheTokenAsync(refreshToken);

            var authTokensDto = new AuthTokensDTO(
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.AccessTokenExpiresAt,
                tokens.RefreshTokenExpiresAt
            );

            return new Response(true, "Login successfully", authTokensDto);
        }

        public async Task<Response> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return new Response(false, "Refresh token is required");
            }

            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (existingToken is null)
            {
                return new Response(false, "Refresh token not found");
            }

            if (!existingToken.IsActive)
            {
                return new Response(false, "Refresh token is inactive or expired");
            }

            var user = existingToken.User;
            var payload = new TokenPayload
            {
                Id = user.Id.ToString(),
                Name = user.FullName ?? user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            var tokens = _tokenService.GenerateAuthTokens(payload);

            var replacementToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = tokens.RefreshToken,
                ExpiresAt = tokens.RefreshTokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            var createResponse = await _refreshTokenRepository.CreateAsync(replacementToken);
            if (!createResponse.Flag)
            {
                return new Response(false, "Unable to issue refresh token", createResponse.Message);
            }

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.RevokedByIp = ipAddress;
            existingToken.ReplacedByToken = replacementToken.Token;

            var revokeResponse = await _refreshTokenRepository.UpdateAsync(existingToken);
            if (!revokeResponse.Flag)
            {
                return new Response(false, "Failed to update refresh token state", revokeResponse.Message);
            }

            await _refreshTokenCache.RemoveTokenAsync(refreshToken);
            await _refreshTokenCache.CacheTokenAsync(replacementToken);

            var authTokensDto = new AuthTokensDTO(
                tokens.AccessToken,
                tokens.RefreshToken,
                tokens.AccessTokenExpiresAt,
                tokens.RefreshTokenExpiresAt
            );

            return new Response(true, "Token refreshed successfully", authTokensDto);
        }

        public async Task<Response> RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return new Response(false, "Refresh token is required");
            }

            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (existingToken is null)
            {
                return new Response(false, "Refresh token not found");
            }

            if (existingToken.RevokedAt is not null)
            {
                return new Response(false, "Refresh token has already been revoked");
            }

            if (DateTime.UtcNow >= existingToken.ExpiresAt)
            {
                return new Response(false, "Refresh token has already expired");
            }

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.RevokedByIp = ipAddress;

            var revokeResponse = await _refreshTokenRepository.UpdateAsync(existingToken);
            return revokeResponse.Flag
                ? await HandleSuccessfulRevokeAsync(existingToken)
                : new Response(false, "Failed to revoke refresh token", revokeResponse.Message);
        }

        private async Task<Response> HandleSuccessfulRevokeAsync(RefreshToken token)
        {
            await _refreshTokenCache.RemoveTokenAsync(token.Token);
            return new Response(true, "Refresh token revoked successfully");
        }

        public async Task<GetUserDTO?> GetByIdAsync(Guid userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            return user is null ? null : MapToDto(user);
        }

        public async Task<IEnumerable<GetUserDTO>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select(MapToDto);
        }

        public async Task<Response> UpdateAsync(Guid userId, UpdateUserDTO userDTO)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user is null)
            {
                return new Response(false, "User not found");
            }

            if (!string.IsNullOrWhiteSpace(userDTO.Email))
            {
                var trimmedEmail = userDTO.Email.Trim();
                if (!string.Equals(trimmedEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var existing = await _userRepository.GetByEmailAsync(trimmedEmail);
                    if (existing is not null && existing.Id != userId)
                    {
                        return new Response(false, "Email already registered");
                    }
                    user.Email = trimmedEmail;
                }
            }

            if (!string.IsNullOrWhiteSpace(userDTO.UserName))
            {
                var trimmedUserName = userDTO.UserName.Trim();
                if (!string.Equals(trimmedUserName, user.UserName, StringComparison.OrdinalIgnoreCase))
                {
                    var usernameExists = await _userRepository.UserNameExistsAsync(trimmedUserName, userId);
                    if (usernameExists)
                    {
                        return new Response(false, "Username already taken");
                    }
                    user.UserName = trimmedUserName;
                }
            }

            if (userDTO.ProfilePicture is not null)
            {
                var incoming = userDTO.ProfilePicture.Trim();

                if (string.IsNullOrWhiteSpace(incoming))
                {
                    await _avatarStorageService.DeleteAvatarAsync(user.ProfilePicture);
                    user.ProfilePicture = null;
                }
                else if (LooksLikeDataUrl(incoming))
                {
                    var newAvatarUrl = await _avatarStorageService.SaveAvatarAsync(user.Id, incoming);
                    await _avatarStorageService.DeleteAvatarAsync(user.ProfilePicture);
                    user.ProfilePicture = newAvatarUrl;
                }
                else
                {
                    user.ProfilePicture = incoming;
                }
            }

            user.FullName = userDTO.FullName ?? user.FullName;
            user.PhoneNumber = userDTO.PhoneNumber ?? user.PhoneNumber;
            user.Gender = userDTO.Gender ?? user.Gender;
            user.DateOfBirth = userDTO.DateOfBirth ?? user.DateOfBirth;
            user.Role = userDTO.Role ?? user.Role;
            user.Status = userDTO.Status ?? user.Status;
            user.SubscriptionPlanId = userDTO.SubscriptionPlanId ?? user.SubscriptionPlanId;
            user.PaymentCustomerRef = userDTO.PaymentCustomerRef ?? user.PaymentCustomerRef;

            if (!string.IsNullOrWhiteSpace(userDTO.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password);
            }

            var updateResponse = await _userRepository.UpdateAsync(user);
            return updateResponse.Flag
                ? new Response(true, "User updated successfully")
                : updateResponse;
        }

        public async Task<Response> UpdateSubscriptionAsync(Guid userId, Guid subscriptionPlanId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user is null)
            {
                return new Response(false, "User not found");
            }

            user.SubscriptionPlanId = subscriptionPlanId;

            var updateResponse = await _userRepository.UpdateAsync(user);
            return updateResponse.Flag
                ? new Response(true, "User subscription updated successfully")
                : updateResponse;
        }

        public async Task<Response> DeleteAsync(Guid userId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user is null)
            {
                return new Response(false, "User not found");
            }

            var deleteResponse = await _userRepository.DeleteAsync(user);
            return deleteResponse.Flag
                ? new Response(true, "User deleted successfully")
                : deleteResponse;
        }

        private static GetUserDTO MapToDto(User user) => new GetUserDTO(
            user.Id,
            user.Email,
            user.UserName,
            user.FullName,
            user.PhoneNumber,
            user.Gender,
            user.ProfilePicture,
            user.DateOfBirth,
            user.Role,
            user.Status,
            user.CreatedAt,
            user.SubscriptionPlanId,
            user.PaymentCustomerRef
        );

        private async Task<string?> ProcessIncomingProfilePictureAsync(Guid userId, string? profilePicture)
        {
            if (string.IsNullOrWhiteSpace(profilePicture))
            {
                return null;
            }

            var trimmed = profilePicture.Trim();

            if (LooksLikeDataUrl(trimmed))
            {
                return await _avatarStorageService.SaveAvatarAsync(userId, trimmed);
            }

            return trimmed;
        }

        private static bool LooksLikeDataUrl(string value)
        {
            return value.StartsWith("data:image", StringComparison.OrdinalIgnoreCase) ||
                   value.Contains(";base64,", StringComparison.OrdinalIgnoreCase);
        }
    }
}


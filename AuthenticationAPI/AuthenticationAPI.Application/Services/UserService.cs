using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationAPI.Application.DTOs;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;
using BrightNess.Library.Jwt;
using BrightNess.Library.Responses;

namespace AuthenticationAPI.Application.Services
{
    public class UserService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenCacheService refreshTokenCacheService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        private readonly IRefreshTokenCacheService _refreshTokenCache = refreshTokenCacheService ?? throw new ArgumentNullException(nameof(refreshTokenCacheService));

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

            var user = new User
            {
                Id = userDTO.Id ?? Guid.NewGuid(),
                Email = userDTO.Email.Trim(),
                UserName = userDTO.UserName.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDTO.Password),
                FullName = userDTO.FullName,
                PhoneNumber = userDTO.PhoneNumber,
                Gender = userDTO.Gender,
                ProfilePicture = userDTO.ProfilePicture,
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

            var payload = new TokenPayload
            {
                Id = user.Id.ToString(), // Thêm Id của user vào payload
                Name = user.FullName ?? user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            var tokens = _tokenService.GenerateAuthTokens(payload);

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

            user.FullName = userDTO.FullName ?? user.FullName;
            user.PhoneNumber = userDTO.PhoneNumber ?? user.PhoneNumber;
            user.Gender = userDTO.Gender ?? user.Gender;
            user.ProfilePicture = userDTO.ProfilePicture ?? user.ProfilePicture;
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
    }
}

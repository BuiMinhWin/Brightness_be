using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AuthenticationAPI.Application.DTOs;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;
using BrightNess.Library.Jwt;
using BrightNess.Library.Responses;
using Microsoft.Extensions.Options;

namespace AuthenticationAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly JwtOptions _jwtOptions;

        public UserService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IOptions<JwtOptions> jwtOptions)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        public async Task<Response> RegisterAsync(UserDTO userDTO)
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

            var tokenPayload = new TokenPayload
            {
                Name = user.FullName ?? user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            var accessToken = _tokenService.GenerateToken(tokenPayload);
            var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

            var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);
            var refreshCreateResponse = await _refreshTokenRepository.CreateAsync(refreshToken);
            if (!refreshCreateResponse.Flag)
            {
                return new Response(false, "Failed to persist refresh token");
            }

            var tokens = new AuthTokensDTO(accessToken, refreshToken.Token, accessTokenExpiresAt, refreshToken.ExpiresAt);
            return new Response(true, "Login successfully", tokens);
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
                return new Response(false, existingToken.RevokedAt.HasValue ? "Refresh token has been revoked" : "Refresh token expired");
            }

            var user = existingToken.User;
            if (user is null)
            {
                return new Response(false, "User not found for refresh token");
            }

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.RevokedByIp = ipAddress;

            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);
            existingToken.ReplacedByToken = newRefreshToken.Token;

            var revokeResponse = await _refreshTokenRepository.UpdateAsync(existingToken);
            if (!revokeResponse.Flag)
            {
                return new Response(false, "Failed to revoke refresh token");
            }

            var createResponse = await _refreshTokenRepository.CreateAsync(newRefreshToken);
            if (!createResponse.Flag)
            {
                return new Response(false, "Failed to persist refresh token");
            }

            var payload = new TokenPayload
            {
                Name = user.FullName ?? user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            var accessToken = _tokenService.GenerateToken(payload);
            var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

            var tokens = new AuthTokensDTO(accessToken, newRefreshToken.Token, accessTokenExpiresAt, newRefreshToken.ExpiresAt);
            return new Response(true, "Token refreshed successfully", tokens);
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

            if (!existingToken.IsActive)
            {
                return new Response(false, "Refresh token is already inactive");
            }

            existingToken.RevokedAt = DateTime.UtcNow;
            existingToken.RevokedByIp = ipAddress;

            var revokeResponse = await _refreshTokenRepository.UpdateAsync(existingToken);
            return revokeResponse.Flag
                ? new Response(true, "Refresh token revoked")
                : revokeResponse;
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

        private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string? ipAddress)
        {
            RefreshToken? refreshToken;
            do
            {
                refreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = ipAddress,
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
                };
            }
            while (await _refreshTokenRepository.GetByTokenAsync(refreshToken.Token) is not null);

            return refreshToken;
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

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
    public class UserService(IUserRepository userRepository, ITokenService tokenService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));

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

            var payload = new TokenPayload
            {
                Id = user.Id.ToString(), // Thêm Id của user vào payload
                Name = user.FullName ?? user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            var tokens = _tokenService.GenerateAuthTokens(payload);
            return new Response(true, "Login successfully", tokens);
        }

        public async Task<Response> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            throw new NotImplementedException();
        }

        public async Task<Response> RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            throw new NotImplementedException();
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

using System;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationAPI.Application.DTOs;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;
using AuthenticationAPI.Infrastructure.Data;
using BrightNess.Library.Jwt;
using BrightNess.Library.Responses;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Infrastructure.Repositories
{
    public class UserRepository(AuthenticationDbContext context, ITokenService tokenService) : IUser
    {
        private async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalizedEmail = email.Trim().ToLowerInvariant();

            return await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Email != null && user.Email.ToLower() == normalizedEmail);
        }

        public async Task<GetUserDTO?> GetUser(Guid userId)
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user is null
                ? null
                : new GetUserDTO(
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

        public async Task<Response> Login(LoginDTO loginDTO)
        {
            var user = await GetUserByEmailAsync(loginDTO.email);
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
                Name = user.FullName ?? user.UserName,
                Email = user.Email,
                Role = user.Role
            };

            var token = tokenService.GenerateToken(payload);
            return new Response(true, token);
        }

        public async Task<Response> Register(UserDTO userDTO)
        {
            var existingByEmail = await GetUserByEmailAsync(userDTO.Email);
            if (existingByEmail is not null)
            {
                return new Response(false, "Email already registered");
            }

            var normalizedUserName = userDTO.UserName.Trim().ToLowerInvariant();
            var usernameExists = await context.Users
                .AsNoTracking()
                .AnyAsync(u => u.UserName != null && u.UserName.ToLower() == normalizedUserName);

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

            await context.Users.AddAsync(user);
            var saveResult = await context.SaveChangesAsync();

            return saveResult > 0
                ? new Response(true, "Register successfully")
                : new Response(false, "Unable to register user");
        }
    }
}

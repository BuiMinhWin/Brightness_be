using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthenticationAPI.Application.DTOs;
using BrightNess.Library.Responses;

namespace AuthenticationAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<Response> RegisterAsync(RegisterDTO registerDTO);
        Task<Response> LoginAsync(LoginDTO loginDTO, string? ipAddress = null);
        Task<Response> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
        Task<Response> RevokeRefreshTokenAsync(string refreshToken, string? ipAddress = null);
        Task<GetUserDTO?> GetByIdAsync(Guid userId);
        Task<IEnumerable<GetUserDTO>> GetAllAsync();
        Task<Response> UpdateAsync(Guid userId, UpdateUserDTO userDTO);
        Task<Response> UpdateSubscriptionAsync(Guid userId, Guid subscriptionPlanId);
        Task<Response> DeleteAsync(Guid userId);
        Task<Response> ExternalLoginAsync(ExternalLoginDTO externalLoginDTO, string? ipAddress = null);
    }
}

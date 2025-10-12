using System;
using System.Threading.Tasks;
using AuthenticationAPI.Domain.Entities;
using BrightNess.Library.Interface;
using BrightNess.Library.Responses;

namespace AuthenticationAPI.Application.Interfaces
{
    public interface IRefreshTokenRepository : IGenericInterface<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<Response> RevokeActiveTokensAsync(Guid userId, string? replacedByToken = null, string? revokedByIp = null);
    }
}

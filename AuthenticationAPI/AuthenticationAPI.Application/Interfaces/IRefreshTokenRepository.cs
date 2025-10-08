using System.Threading.Tasks;
using AuthenticationAPI.Domain.Entities;
using BrightNess.Library.Interface;

namespace AuthenticationAPI.Application.Interfaces
{
    public interface IRefreshTokenRepository : IGenericInterface<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
    }
}

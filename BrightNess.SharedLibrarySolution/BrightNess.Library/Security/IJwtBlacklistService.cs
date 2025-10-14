using System;
using System.Threading.Tasks;

namespace BrightNess.Library.Security
{
    public interface IJwtBlacklistService
    {
        Task BlacklistTokenAsync(string jti, DateTime expiry);
        Task<bool> IsTokenBlacklistedAsync(string jti);
        Task BlacklistUserTokensAsync(string userId);
    }
}
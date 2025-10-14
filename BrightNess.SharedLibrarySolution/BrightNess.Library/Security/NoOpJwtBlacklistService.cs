using System.Threading.Tasks;
using BrightNess.Library.Security;

namespace BrightNess.Library.Security
{
    public class NoOpJwtBlacklistService : IJwtBlacklistService
    {
        public Task BlacklistTokenAsync(string jti, System.DateTime expiry) => Task.CompletedTask;

        public Task<bool> IsTokenBlacklistedAsync(string jti) => Task.FromResult(false);

        public Task BlacklistUserTokensAsync(string userId) => Task.CompletedTask;
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;
using AuthenticationAPI.Infrastructure.Data;
using BrightNess.Library.Responses;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AuthenticationDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await Context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<Response> RevokeActiveTokensAsync(Guid userId, string? replacedByToken = null, string? revokedByIp = null)
        {
            var activeTokens = await Context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            if (activeTokens.Count == 0)
            {
                return new Response(true, "No active refresh tokens to revoke");
            }

            var revokedAt = DateTime.UtcNow;
            foreach (var token in activeTokens)
            {
                token.RevokedAt = revokedAt;
                token.RevokedByIp = revokedByIp;
                token.ReplacedByToken = replacedByToken;
            }

            Context.RefreshTokens.UpdateRange(activeTokens);
            var rows = await Context.SaveChangesAsync();

            return rows > 0
                ? new Response(true, "Active refresh tokens revoked successfully")
                : new Response(false, "No changes were saved");
        }
    }
}

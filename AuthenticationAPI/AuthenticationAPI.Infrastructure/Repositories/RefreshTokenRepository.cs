using System.Threading.Tasks;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;
using AuthenticationAPI.Infrastructure.Data;
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
    }
}

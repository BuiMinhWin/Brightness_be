using System;
using System.Threading.Tasks;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;
using AuthenticationAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Infrastructure.Repositories
{
    public class UserExternalLoginRepository : IUserExternalLoginRepository
    {
        private readonly AuthenticationDbContext _context;

        public UserExternalLoginRepository(AuthenticationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<UserExternalLogin?> GetByProviderAsync(string provider, string providerKey)
        {
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(providerKey))
            {
                return null;
            }

            var normalizedProvider = provider.Trim().ToLowerInvariant();
            var normalizedKey = providerKey.Trim();

            return await _context.UserExternalLogins
                .AsNoTracking()
                .Include(l => l.User)
                .FirstOrDefaultAsync(l =>
                    l.Provider.ToLower() == normalizedProvider &&
                    l.ProviderKey == normalizedKey);
        }

        public async Task<UserExternalLogin?> GetByEmailAsync(string provider, string email)
        {
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedProvider = provider.Trim().ToLowerInvariant();
            var normalizedEmail = email.Trim().ToLowerInvariant();

            return await _context.UserExternalLogins
                .AsNoTracking()
                .Include(l => l.User)
                .FirstOrDefaultAsync(l =>
                    l.Provider.ToLower() == normalizedProvider &&
                    l.Email.ToLower() == normalizedEmail);
        }

        public async Task<UserExternalLogin?> GetByUserAsync(string provider, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                return null;
            }

            var normalizedProvider = provider.Trim().ToLowerInvariant();

            return await _context.UserExternalLogins
                .AsNoTracking()
                .FirstOrDefaultAsync(l =>
                    l.Provider.ToLower() == normalizedProvider &&
                    l.UserId == userId);
        }

        public async Task<UserExternalLogin?> CreateAsync(UserExternalLogin externalLogin)
        {
            await _context.UserExternalLogins.AddAsync(externalLogin);
            await _context.SaveChangesAsync();
            return externalLogin;
        }
    }
}

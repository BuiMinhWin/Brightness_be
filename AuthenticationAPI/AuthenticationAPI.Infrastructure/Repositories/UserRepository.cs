using System;
using System.Threading.Tasks;
using AuthenticationAPI.Application.Interfaces;
using AuthenticationAPI.Domain.Entities;
using AuthenticationAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationAPI.Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AuthenticationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalizedEmail = email.Trim().ToLowerInvariant();

            return await Context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Email != null && user.Email.ToLower() == normalizedEmail);
        }

        public async Task<bool> UserNameExistsAsync(string userName, Guid? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(userName)) return false;
            var normalizedUserName = userName.Trim().ToLowerInvariant();

            return await Context.Users
                .AsNoTracking()
                .AnyAsync(user =>
                    user.UserName != null &&
                    user.UserName.ToLower() == normalizedUserName &&
                    (!excludeUserId.HasValue || user.Id != excludeUserId.Value));
        }
    }
}

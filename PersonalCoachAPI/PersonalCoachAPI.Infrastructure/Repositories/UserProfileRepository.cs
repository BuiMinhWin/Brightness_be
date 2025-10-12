using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;

namespace PersonalCoachAPI.Infrastructure.Repositories
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(Guid userId);
        Task<UserProfile> CreateAsync(UserProfile profile);
        Task<UserProfile> UpdateAsync(UserProfile profile);
        Task<UserPreference?> GetPreferenceByUserIdAsync(Guid userId);
        Task<UserPreference> CreatePreferenceAsync(UserPreference preference);
        Task<UserPreference> UpdatePreferenceAsync(UserPreference preference);
    }

    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly PersonalCoachDbContext _context;

        public UserProfileRepository(PersonalCoachDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfile?> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<UserProfile> CreateAsync(UserProfile profile)
        {
            profile.Id = Guid.NewGuid();
            profile.CreatedAt = DateTime.UtcNow;
            
            await _context.UserProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();
            
            return profile;
        }

        public async Task<UserProfile> UpdateAsync(UserProfile profile)
        {
            profile.UpdatedAt = DateTime.UtcNow;
            
            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();
            
            return profile;
        }

        public async Task<UserPreference?> GetPreferenceByUserIdAsync(Guid userId)
        {
            return await _context.UserPreferences
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<UserPreference> CreatePreferenceAsync(UserPreference preference)
        {
            preference.Id = Guid.NewGuid();
            preference.CreatedAt = DateTime.UtcNow;
            
            await _context.UserPreferences.AddAsync(preference);
            await _context.SaveChangesAsync();
            
            return preference;
        }

        public async Task<UserPreference> UpdatePreferenceAsync(UserPreference preference)
        {
            preference.UpdatedAt = DateTime.UtcNow;
            
            _context.UserPreferences.Update(preference);
            await _context.SaveChangesAsync();
            
            return preference;
        }
    }
}


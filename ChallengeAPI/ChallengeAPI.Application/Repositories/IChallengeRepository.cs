
using ChallengeAPI.Domain.Entities;

namespace ChallengeAPI.Application.Repositories
{
    public interface IChallengeRepository
    {
        Task<Challenge> AddAsync(Challenge challenge);
        Task<Challenge> GetByIdAsync(Guid id);
        Task<IEnumerable<Challenge>> GetAllAsync();
        Task UpdateAsync(Challenge challenge);
        Task DeleteAsync(Challenge challenge);
    }
}

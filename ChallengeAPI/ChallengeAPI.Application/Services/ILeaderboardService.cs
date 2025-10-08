
using ChallengeAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Services
{
    public interface ILeaderboardService
    {
        Task<IEnumerable<LeaderboardDTO>> GetLeaderboardAsync(Guid challengeId);
        Task<Dictionary<Guid, IEnumerable<LeaderboardDTO>>> GetAllLeaderboardsAsync();
        Task<LeaderboardDTO> GetUserRankAsync(Guid challengeId, Guid userId);
        Task ResetLeaderboardAsync(Guid challengeId);
    }
}

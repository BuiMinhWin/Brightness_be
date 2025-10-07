
using ChallengeAPI.Application.DTOs;
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IChallengeRepository _challengeRepository;
        private readonly IChallengeParticipantRepository _challengeParticipantRepository;
        private readonly IChallengeTaskProgressRepository _challengeTaskProgressRepository;
        private readonly IChallengeTaskRepository _challengeTaskRepository;

        public LeaderboardService(IChallengeRepository challengeRepository, IChallengeParticipantRepository challengeParticipantRepository, IChallengeTaskProgressRepository challengeTaskProgressRepository, IChallengeTaskRepository challengeTaskRepository)
        {
            _challengeRepository = challengeRepository;
            _challengeParticipantRepository = challengeParticipantRepository;
            _challengeTaskProgressRepository = challengeTaskProgressRepository;
            _challengeTaskRepository = challengeTaskRepository;
        }

        public async Task<IEnumerable<LeaderboardDTO>> GetLeaderboardAsync(Guid challengeId)
        {
            var participants = await _challengeParticipantRepository.GetByChallengeIdAsync(challengeId);

            var leaderboard = participants.Select(p => new LeaderboardDTO
            {
                UserId = p.UserId,
                UserName = "", // I will need to get the user name from the user service
                Score = p.TotalPoints ?? 0
            }).ToList();

            var rankedLeaderboard = leaderboard.OrderByDescending(l => l.Score)
                                               .Select((l, index) => new LeaderboardDTO
                                               {
                                                   UserId = l.UserId,
                                                   UserName = l.UserName,
                                                   Rank = index + 1,
                                                   Score = l.Score
                                               }).ToList();

            return rankedLeaderboard;
        }

        public async Task<Dictionary<Guid, IEnumerable<LeaderboardDTO>>> GetAllLeaderboardsAsync()
        {
            var challenges = await _challengeRepository.GetAllAsync();
            var allLeaderboards = new Dictionary<Guid, IEnumerable<LeaderboardDTO>>();

            foreach (var challenge in challenges)
            {
                var leaderboard = await GetLeaderboardAsync(challenge.Id);
                allLeaderboards.Add(challenge.Id, leaderboard);
            }

            return allLeaderboards;
        }

        public async Task<LeaderboardDTO> GetUserRankAsync(Guid challengeId, Guid userId)
        {
            var leaderboard = await GetLeaderboardAsync(challengeId);
            var userRank = leaderboard.FirstOrDefault(l => l.UserId == userId);

            return userRank;
        }

        public async Task ResetLeaderboardAsync(Guid challengeId)
        {
            var participants = await _challengeParticipantRepository.GetByChallengeIdAsync(challengeId);
            foreach (var participant in participants)
            {
                var progresses = await _challengeTaskProgressRepository.GetByParticipantIdAsync(participant.Id);
                foreach (var progress in progresses)
                {
                    await _challengeTaskProgressRepository.DeleteAsync(progress);
                }
            }
        }
    }
}

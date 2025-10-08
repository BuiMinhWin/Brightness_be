
using ChallengeAPI.Application.DTOs;
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Application.Services;
using ChallengeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.ServicesImpl
{
    public class ChallengeParticipantService : IChallengeParticipantService
    {
        private readonly IChallengeParticipantRepository _challengeParticipantRepository;
        private readonly IChallengeRepository _challengeRepository;

        public ChallengeParticipantService(IChallengeParticipantRepository challengeParticipantRepository, IChallengeRepository challengeRepository)
        {
            _challengeParticipantRepository = challengeParticipantRepository;
            _challengeRepository = challengeRepository;
        }

        public async Task<ChallengeParticipantDTO> CreateChallengeParticipantAsync(ChallengeParticipantDTO challengeParticipantDto)
        {
            var challenge = await _challengeRepository.GetByIdAsync(challengeParticipantDto.ChallengeId);
            if (challenge == null)
            {
                throw new ArgumentException("Challenge not found.");
            }

            var challengeParticipant = new ChallengeParticipant
            {
                ChallengeId = challengeParticipantDto.ChallengeId,
                UserId = challengeParticipantDto.UserId,
                JoinedAt = DateTime.UtcNow,
                Status = challengeParticipantDto.Status,
                TotalPoints = challengeParticipantDto.TotalPoints,
                PercentageCompleted = challengeParticipantDto.PercentageCompleted,
                CurrentStreak = challengeParticipantDto.CurrentStreak,
                LongestStreak = challengeParticipantDto.LongestStreak
            };

            var createdChallengeParticipant = await _challengeParticipantRepository.AddAsync(challengeParticipant);
            return new ChallengeParticipantDTO(createdChallengeParticipant.Id, createdChallengeParticipant.ChallengeId, createdChallengeParticipant.UserId, createdChallengeParticipant.JoinedAt, createdChallengeParticipant.Status, createdChallengeParticipant.TotalPoints, createdChallengeParticipant.PercentageCompleted, createdChallengeParticipant.CurrentStreak, createdChallengeParticipant.LongestStreak);
        }

        public async Task<ChallengeParticipantDTO> GetChallengeParticipantByIdAsync(Guid id)
        {
            var challengeParticipant = await _challengeParticipantRepository.GetByIdAsync(id);
            if (challengeParticipant == null)
            {
                return null;
            }
            return new ChallengeParticipantDTO(challengeParticipant.Id, challengeParticipant.ChallengeId, challengeParticipant.UserId, challengeParticipant.JoinedAt, challengeParticipant.Status, challengeParticipant.TotalPoints, challengeParticipant.PercentageCompleted, challengeParticipant.CurrentStreak, challengeParticipant.LongestStreak);
        }

        public async Task<IEnumerable<ChallengeParticipantDTO>> GetAllChallengeParticipantsAsync()
        {
            var challengeParticipants = await _challengeParticipantRepository.GetAllAsync();
            return challengeParticipants.Select(cp => new ChallengeParticipantDTO(cp.Id, cp.ChallengeId, cp.UserId, cp.JoinedAt, cp.Status, cp.TotalPoints, cp.PercentageCompleted, cp.CurrentStreak, cp.LongestStreak));
        }

        public async Task<IEnumerable<ChallengeParticipantDTO>> GetParticipantsByChallengeIdAsync(Guid challengeId)
        {
            var challengeParticipants = await _challengeParticipantRepository.GetByChallengeIdAsync(challengeId);
            return challengeParticipants.Select(cp => new ChallengeParticipantDTO(cp.Id, cp.ChallengeId, cp.UserId, cp.JoinedAt, cp.Status, cp.TotalPoints, cp.PercentageCompleted, cp.CurrentStreak, cp.LongestStreak));
        }

        public async Task UpdateChallengeParticipantAsync(Guid id, ChallengeParticipantDTO challengeParticipantDto)
        {
            var existingChallengeParticipant = await _challengeParticipantRepository.GetByIdAsync(id);
            if (existingChallengeParticipant == null)
            {
                throw new ArgumentException("Challenge participant not found.");
            }

            if (challengeParticipantDto.Status != null)
            {
                existingChallengeParticipant.Status = challengeParticipantDto.Status;
            }
            if (challengeParticipantDto.TotalPoints.HasValue)
            {
                existingChallengeParticipant.TotalPoints = challengeParticipantDto.TotalPoints;
            }
            if (challengeParticipantDto.PercentageCompleted.HasValue)
            {
                existingChallengeParticipant.PercentageCompleted = challengeParticipantDto.PercentageCompleted;
            }
            if (challengeParticipantDto.CurrentStreak.HasValue)
            {
                existingChallengeParticipant.CurrentStreak = challengeParticipantDto.CurrentStreak;
            }
            if (challengeParticipantDto.LongestStreak.HasValue)
            {
                existingChallengeParticipant.LongestStreak = challengeParticipantDto.LongestStreak;
            }

            await _challengeParticipantRepository.UpdateAsync(existingChallengeParticipant);
        }

        public async Task DeleteChallengeParticipantAsync(Guid id)
        {
            var existingChallengeParticipant = await _challengeParticipantRepository.GetByIdAsync(id);
            if (existingChallengeParticipant == null)
            {
                throw new ArgumentException("Challenge participant not found.");
            }

            await _challengeParticipantRepository.DeleteAsync(existingChallengeParticipant);
        }
    }
}


using ChallengeAPI.Application.DTOs;
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Application.Services;
using ChallengeAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.ServicesImpl
{
    public class ChallengeTaskProgressService : IChallengeTaskProgressService
    {
        private readonly IChallengeTaskProgressRepository _challengeTaskProgressRepository;
        private readonly IChallengeParticipantRepository _challengeParticipantRepository;
        private readonly IChallengeTaskRepository _challengeTaskRepository;

        public ChallengeTaskProgressService(IChallengeTaskProgressRepository challengeTaskProgressRepository, IChallengeParticipantRepository challengeParticipantRepository, IChallengeTaskRepository challengeTaskRepository)
        {
            _challengeTaskProgressRepository = challengeTaskProgressRepository;
            _challengeParticipantRepository = challengeParticipantRepository;
            _challengeTaskRepository = challengeTaskRepository;
        }

        public async Task<ChallengeTaskProgressDTO> CreateChallengeTaskProgressAsync(ChallengeTaskProgressDTO challengeTaskProgressDto)
        {
            var participant = await _challengeParticipantRepository.GetByIdAsync(challengeTaskProgressDto.ParticipantId);
            if (participant == null)
            {
                throw new ArgumentException("Participant not found.");
            }

            var task = await _challengeTaskRepository.GetByIdAsync(challengeTaskProgressDto.ChallengeTaskId);
            if (task == null)
            {
                throw new ArgumentException("Task not found.");
            }

            var challengeTaskProgress = new ChallengeTaskProgress
            {
                ParticipantId = challengeTaskProgressDto.ParticipantId,
                ChallengeTaskId = challengeTaskProgressDto.ChallengeTaskId,
                TargetDate = challengeTaskProgressDto.TargetDate,
                Status = challengeTaskProgressDto.Status ?? "pending",
                ValidationData = challengeTaskProgressDto.ValidationData,
                PointsEarned = challengeTaskProgressDto.PointsEarned ?? 0,
                CompletedAt = challengeTaskProgressDto.CompletedAt
            };

            var createdChallengeTaskProgress = await _challengeTaskProgressRepository.AddAsync(challengeTaskProgress);
            await UpdateParticipantTotalPointsAsync(participant.Id);

            return new ChallengeTaskProgressDTO(
                createdChallengeTaskProgress.Id,
                createdChallengeTaskProgress.ParticipantId,
                createdChallengeTaskProgress.ChallengeTaskId,
                createdChallengeTaskProgress.TargetDate,
                createdChallengeTaskProgress.Status,
                createdChallengeTaskProgress.ValidationData,
                createdChallengeTaskProgress.PointsEarned,
                createdChallengeTaskProgress.CompletedAt
            );
        }

        public async Task<ChallengeTaskProgressDTO> GetChallengeTaskProgressByIdAsync(Guid id)
        {
            var challengeTaskProgress = await _challengeTaskProgressRepository.GetByIdAsync(id);
            if (challengeTaskProgress == null)
            {
                return null;
            }
            return new ChallengeTaskProgressDTO(
                challengeTaskProgress.Id,
                challengeTaskProgress.ParticipantId,
                challengeTaskProgress.ChallengeTaskId,
                challengeTaskProgress.TargetDate,
                challengeTaskProgress.Status,
                challengeTaskProgress.ValidationData,
                challengeTaskProgress.PointsEarned,
                challengeTaskProgress.CompletedAt
            );
        }

        public async Task<IEnumerable<ChallengeTaskProgressDTO>> GetAllChallengeTaskProgressesAsync()
        {
            var challengeTaskProgresses = await _challengeTaskProgressRepository.GetAllAsync();
            return challengeTaskProgresses.Select(ctp => new ChallengeTaskProgressDTO(
                ctp.Id,
                ctp.ParticipantId,
                ctp.ChallengeTaskId,
                ctp.TargetDate,
                ctp.Status,
                ctp.ValidationData,
                ctp.PointsEarned,
                ctp.CompletedAt
            ));
        }

        public async Task<IEnumerable<ChallengeTaskProgressDTO>> GetProgressByParticipantIdAsync(Guid participantId)
        {
            var challengeTaskProgresses = await _challengeTaskProgressRepository.GetByParticipantIdAsync(participantId);
            return challengeTaskProgresses.Select(ctp => new ChallengeTaskProgressDTO(
                ctp.Id,
                ctp.ParticipantId,
                ctp.ChallengeTaskId,
                ctp.TargetDate,
                ctp.Status,
                ctp.ValidationData,
                ctp.PointsEarned,
                ctp.CompletedAt
            ));
        }

        public async Task UpdateChallengeTaskProgressAsync(Guid id, ChallengeTaskProgressDTO challengeTaskProgressDto)
        {
            var existingChallengeTaskProgress = await _challengeTaskProgressRepository.GetByIdAsync(id);
            if (existingChallengeTaskProgress == null)
            {
                throw new ArgumentException("Challenge task progress not found.");
            }

            if (challengeTaskProgressDto.TargetDate.HasValue)
            {
                existingChallengeTaskProgress.TargetDate = challengeTaskProgressDto.TargetDate;
            }
            if (challengeTaskProgressDto.Status != null)
            {
                existingChallengeTaskProgress.Status = challengeTaskProgressDto.Status;
            }
            if (challengeTaskProgressDto.ValidationData != null)
            {
                existingChallengeTaskProgress.ValidationData = challengeTaskProgressDto.ValidationData;
            }
            if (challengeTaskProgressDto.PointsEarned.HasValue)
            {
                existingChallengeTaskProgress.PointsEarned = challengeTaskProgressDto.PointsEarned;
            }
            if (challengeTaskProgressDto.CompletedAt.HasValue)
            {
                existingChallengeTaskProgress.CompletedAt = challengeTaskProgressDto.CompletedAt;
            }

            await _challengeTaskProgressRepository.UpdateAsync(existingChallengeTaskProgress);
            await UpdateParticipantTotalPointsAsync(existingChallengeTaskProgress.ParticipantId);
        }

        public async Task PatchChallengeTaskProgressAsync(Guid id, Dictionary<string, object> patchData)
        {
            var existingChallengeTaskProgress = await _challengeTaskProgressRepository.GetByIdAsync(id);
            if (existingChallengeTaskProgress == null)
            {
                throw new ArgumentException("Challenge task progress not found.");
            }

            foreach (var kvp in patchData)
            {
                var propertyName = kvp.Key.ToLower();
                var value = kvp.Value;

                switch (propertyName)
                {
                    case "targetdate":
                        if (DateOnly.TryParse(value.ToString(), out var targetDate))
                        {
                            existingChallengeTaskProgress.TargetDate = targetDate;
                        }
                        break;
                    case "status":
                        existingChallengeTaskProgress.Status = value.ToString();
                        break;
                    case "validationdata":
                        existingChallengeTaskProgress.ValidationData = value.ToString();
                        break;
                    case "pointsearned":
                        if (decimal.TryParse(value.ToString(), out var pointsEarned))
                        {
                            existingChallengeTaskProgress.PointsEarned = pointsEarned;
                        }
                        break;
                    case "completedat":
                        if (DateTime.TryParse(value.ToString(), out var completedAt))
                        {
                            existingChallengeTaskProgress.CompletedAt = completedAt;
                        }
                        break;
                }
            }

            await _challengeTaskProgressRepository.UpdateAsync(existingChallengeTaskProgress);
            await UpdateParticipantTotalPointsAsync(existingChallengeTaskProgress.ParticipantId);
        }

        public async Task DeleteChallengeTaskProgressAsync(Guid id)
        {
            var existingChallengeTaskProgress = await _challengeTaskProgressRepository.GetByIdAsync(id);
            if (existingChallengeTaskProgress == null)
            {
                throw new ArgumentException("Challenge task progress not found.");
            }

            await _challengeTaskProgressRepository.DeleteAsync(existingChallengeTaskProgress);
            await UpdateParticipantTotalPointsAsync(existingChallengeTaskProgress.ParticipantId);
        }

        private async Task UpdateParticipantTotalPointsAsync(Guid participantId)
        {
            var progresses = await _challengeTaskProgressRepository.GetByParticipantIdAsync(participantId);
            var totalPoints = progresses.Sum(p => p.PointsEarned ?? 0);

            var participant = await _challengeParticipantRepository.GetByIdAsync(participantId);
            if (participant != null)
            {
                participant.TotalPoints = totalPoints;
                await _challengeParticipantRepository.UpdateAsync(participant);
            }
        }
    }
}

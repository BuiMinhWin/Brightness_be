//using ChallengeAPI.Application.DTOs;
//using ChallengeAPI.Application.Repositories;
//using ChallengeAPI.Application.Services;
//using ChallengeAPI.Domain.Entities;
//using Microsoft.AspNetCore.JsonPatch;

//namespace ChallengeAPI.Application.ServicesImpl
//{
//    public class ChallengeService : IChallengeService
//    {
//        private readonly IChallengeRepository _challengeRepository;

//        public ChallengeService(IChallengeRepository challengeRepository)
//        {
//            _challengeRepository = challengeRepository;
//        }

//        public async Task<Challenge> CreateChallengeAsync(ChallengeDTO challengeDto)
//        {
//            if (challengeDto.StartAt.HasValue && challengeDto.EndAt.HasValue && challengeDto.StartAt.Value >= challengeDto.EndAt.Value)
//            {
//                throw new ArgumentException("Start date must be before end date.");
//            }

//            if (challengeDto.StartAt.HasValue && challengeDto.StartAt.Value < DateTime.UtcNow)
//            {
//                throw new ArgumentException("Start date cannot be in the past.");
//            }

//            var titleWords = challengeDto.Title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//            if (titleWords.Length > 15)
//            {
//                throw new ArgumentException("Title cannot have more than 15 words.");
//            }

//            if (!string.IsNullOrEmpty(challengeDto.Description))
//            {
//                var descriptionWords = challengeDto.Description.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//                if (descriptionWords.Length > 100)
//                {
//                    throw new ArgumentException("Description cannot have more than 100 words.");
//                }
//            }

//            var challenge = new Challenge
//            {
//                HostUserId = challengeDto.HostUserId,
//                Title = challengeDto.Title,
//                Description = challengeDto.Description,
//                StartAt = challengeDto.StartAt,
//                EndAt = challengeDto.EndAt,
//                Status = challengeDto.Status,
//                MaxParticipants = challengeDto.MaxParticipants,
//                PrizePool = challengeDto.PrizePool,
//                Category = challengeDto.Category,
//                TaskSummary = challengeDto.TaskSummary,
//                CreatedAt = DateTime.UtcNow,
//                UpdatedAt = DateTime.UtcNow
//            };

//            return await _challengeRepository.AddAsync(challenge);
//        }

//        public async Task UpdateChallengeAsync(Guid id, ChallengeDTO challengeDto)
//        {
//            var existingChallenge = await _challengeRepository.GetByIdAsync(id);
//            if (existingChallenge == null)
//            {
//                throw new ArgumentException("Challenge not found.");
//            }

//            existingChallenge.Title = challengeDto.Title;
//            existingChallenge.Description = challengeDto.Description;
//            existingChallenge.StartAt = challengeDto.StartAt;
//            existingChallenge.EndAt = challengeDto.EndAt;
//            existingChallenge.Status = challengeDto.Status;
//            existingChallenge.MaxParticipants = challengeDto.MaxParticipants;
//            existingChallenge.PrizePool = challengeDto.PrizePool;
//            existingChallenge.Category = challengeDto.Category;
//            existingChallenge.TaskSummary = challengeDto.TaskSummary;
//            existingChallenge.UpdatedAt = DateTime.UtcNow;

//            await _challengeRepository.UpdateAsync(existingChallenge);
//        }


//        public async Task PatchChallengeAsync(Guid id, Dictionary<string, object> patchData)
//        {
//            var existingChallenge = await _challengeRepository.GetByIdAsync(id);
//            if (existingChallenge == null)
//            {
//                throw new ArgumentException("Challenge not found.");
//            }

//            // CHỈ update những field có trong dictionary
//            foreach (var kvp in patchData)
//            {
//                var propertyName = kvp.Key.ToLower();
//                var value = kvp.Value;

//                switch (propertyName)
//                {
//                    case "hostuserid":
//                        if (value != null)
//                            existingChallenge.HostUserId = Guid.Parse(value.ToString());
//                        break;
//                    case "title":
//                        if (value != null)
//                            existingChallenge.Title = value.ToString();
//                        break;
//                    case "description":
//                        existingChallenge.Description = value?.ToString();
//                        break;
//                    case "startat":
//                        existingChallenge.StartAt = value != null
//                            ? DateTime.SpecifyKind(DateTime.Parse(value.ToString()), DateTimeKind.Utc)
//                            : null;
//                        break;
//                    case "endat":
//                        existingChallenge.EndAt = value != null
//                            ? DateTime.SpecifyKind(DateTime.Parse(value.ToString()), DateTimeKind.Utc)
//                            : null;
//                        break;
//                    case "status":
//                        if (value != null)
//                            existingChallenge.Status = value.ToString();
//                        break;
//                    case "maxparticipants":
//                        existingChallenge.MaxParticipants = value != null ? Convert.ToInt32(value) : null;
//                        break;
//                    case "prizepool":
//                        existingChallenge.PrizePool = value != null ? Convert.ToDecimal(value) : null;
//                        break;
//                    case "category":
//                        existingChallenge.Category = value?.ToString();
//                        break;
//                    case "tasksummary":
//                        existingChallenge.TaskSummary = value?.ToString();
//                        break;
//                }
//            }

//            existingChallenge.UpdatedAt = DateTime.UtcNow;
//            await _challengeRepository.UpdateAsync(existingChallenge);
//        }


//        public async Task DeleteChallengeAsync(Guid id)
//        {
//            var existingChallenge = await _challengeRepository.GetByIdAsync(id);
//            if (existingChallenge == null)
//            {
//                throw new ArgumentException("Challenge not found.");
//            }
//            await _challengeRepository.DeleteAsync(existingChallenge);
//        }



//        public async Task<IEnumerable<ChallengeDTO>> GetAllChallengesAsync()
//        {
//            var challenges = await _challengeRepository.GetAllAsync();
//            return challenges.Select(challenge => new ChallengeDTO(
//                challenge.Id,
//                challenge.HostUserId,
//                challenge.Title,
//                challenge.Description,
//                challenge.StartAt,
//                challenge.EndAt,
//                challenge.Status,
//                challenge.MaxParticipants,
//                challenge.PrizePool,
//                challenge.Category,
//                challenge.TaskSummary,
//                challenge.CreatedAt,
//                challenge.UpdatedAt
//            ));
//        }



//        public async Task<ChallengeDTO> GetChallengeByIdAsync(Guid id)
//        {
//            var challenge = await _challengeRepository.GetByIdAsync(id);
//            if (challenge == null)
//            {
//                return null;
//            }

//            return new ChallengeDTO(
//                challenge.Id, 
//                challenge.HostUserId,
//                challenge.Title,
//                challenge.Description,
//                challenge.StartAt,
//                challenge.EndAt,
//                challenge.Status,
//                challenge.MaxParticipants,
//                challenge.PrizePool,
//                challenge.Category,
//                challenge.TaskSummary,
//                challenge.CreatedAt,
//                challenge.UpdatedAt
//            );
//        }


//    }
//}

using ChallengeAPI.Application.DTOs;
using ChallengeAPI.Application.Repositories;
using ChallengeAPI.Application.Services;
using ChallengeAPI.Domain.Entities;
using Microsoft.AspNetCore.JsonPatch;

namespace ChallengeAPI.Application.ServicesImpl
{
    public class ChallengeService : IChallengeService
    {
        private readonly IChallengeRepository _challengeRepository;

        public ChallengeService(IChallengeRepository challengeRepository)
        {
            _challengeRepository = challengeRepository;
        }

        public async Task<Challenge> CreateChallengeAsync(ChallengeDTO challengeDto)
        {
            if (challengeDto.StartAt.HasValue && challengeDto.EndAt.HasValue && challengeDto.StartAt.Value >= challengeDto.EndAt.Value)
            {
                throw new ArgumentException("Start date must be before end date.");
            }

            if (challengeDto.StartAt.HasValue && challengeDto.StartAt.Value < DateTime.UtcNow)
            {
                throw new ArgumentException("Start date cannot be in the past.");
            }

            var titleWords = challengeDto.Title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (titleWords.Length > 15)
            {
                throw new ArgumentException("Title cannot have more than 15 words.");
            }

            if (!string.IsNullOrEmpty(challengeDto.Description))
            {
                var descriptionWords = challengeDto.Description.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (descriptionWords.Length > 100)
                {
                    throw new ArgumentException("Description cannot have more than 100 words.");
                }
            }

            var challenge = new Challenge
            {
                HostUserId = challengeDto.HostUserId,
                Title = challengeDto.Title,
                Description = challengeDto.Description,
                StartAt = challengeDto.StartAt,
                EndAt = challengeDto.EndAt,
                Status = challengeDto.Status,
                MaxParticipants = challengeDto.MaxParticipants,
                PrizePool = challengeDto.PrizePool,
                Category = challengeDto.Category,
                TaskSummary = challengeDto.TaskSummary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _challengeRepository.AddAsync(challenge);
        }

        public async Task UpdateChallengeAsync(Guid id, ChallengeDTO challengeDto)
        {
            var existingChallenge = await _challengeRepository.GetByIdAsync(id);
            if (existingChallenge == null)
            {
                throw new ArgumentException("Challenge not found.");
            }

            existingChallenge.Title = challengeDto.Title;
            existingChallenge.Description = challengeDto.Description;
            existingChallenge.StartAt = challengeDto.StartAt;
            existingChallenge.EndAt = challengeDto.EndAt;
            existingChallenge.Status = challengeDto.Status;
            existingChallenge.MaxParticipants = challengeDto.MaxParticipants;
            existingChallenge.PrizePool = challengeDto.PrizePool;
            existingChallenge.Category = challengeDto.Category;
            existingChallenge.TaskSummary = challengeDto.TaskSummary;
            existingChallenge.UpdatedAt = DateTime.UtcNow;

            await _challengeRepository.UpdateAsync(existingChallenge);
        }

        public async Task PatchChallengeAsync(Guid id, Dictionary<string, object> patchData)
        {
            var existingChallenge = await _challengeRepository.GetByIdAsync(id);
            if (existingChallenge == null)
            {
                throw new ArgumentException("Challenge not found.");
            }

            foreach (var kvp in patchData)
            {
                var propertyName = kvp.Key.ToLower();
                var value = kvp.Value;

                switch (propertyName)
                {
                    case "hostuserid":
                        if (value != null)
                            existingChallenge.HostUserId = Guid.Parse(value.ToString());
                        break;
                    case "title":
                        if (value != null)
                            existingChallenge.Title = value.ToString();
                        break;
                    case "description":
                        existingChallenge.Description = value?.ToString();
                        break;
                    case "startat":
                        existingChallenge.StartAt = value != null
                            ? DateTime.SpecifyKind(DateTime.Parse(value.ToString()), DateTimeKind.Utc)
                            : null;
                        break;
                    case "endat":
                        existingChallenge.EndAt = value != null
                            ? DateTime.SpecifyKind(DateTime.Parse(value.ToString()), DateTimeKind.Utc)
                            : null;
                        break;
                    case "status":
                        if (value != null)
                            existingChallenge.Status = value.ToString();
                        break;
                    case "maxparticipants":
                        existingChallenge.MaxParticipants = value != null ? Convert.ToInt32(value) : null;
                        break;
                    case "prizepool":
                        existingChallenge.PrizePool = value != null ? Convert.ToDecimal(value) : null;
                        break;
                    case "category":
                        existingChallenge.Category = value?.ToString();
                        break;
                    case "tasksummary":
                        existingChallenge.TaskSummary = value?.ToString();
                        break;
                }
            }

            existingChallenge.UpdatedAt = DateTime.UtcNow;
            await _challengeRepository.UpdateAsync(existingChallenge);
        }

        public async Task DeleteChallengeAsync(Guid id)
        {
            var existingChallenge = await _challengeRepository.GetByIdAsync(id);
            if (existingChallenge == null)
            {
                throw new ArgumentException("Challenge not found.");
            }
            await _challengeRepository.DeleteAsync(existingChallenge);
        }

        public async Task<IEnumerable<ChallengeDTO>> GetAllChallengesAsync()
        {
            var challenges = await _challengeRepository.GetAllAsync();
            return challenges.Select(challenge => new ChallengeDTO(
                challenge.Id,              // ✅ FIXED: Id is now first parameter
                challenge.HostUserId,
                challenge.Title,
                challenge.Description,
                challenge.StartAt,
                challenge.EndAt,
                challenge.Status,
                challenge.MaxParticipants,
                challenge.PrizePool,
                challenge.Category,
                challenge.TaskSummary,
                challenge.CreatedAt,
                challenge.UpdatedAt
            ));
        }

        public async Task<ChallengeDTO> GetChallengeByIdAsync(Guid id)
        {
            var challenge = await _challengeRepository.GetByIdAsync(id);
            if (challenge == null)
            {
                return null;
            }

            return new ChallengeDTO(
                challenge.Id,              // ✅ FIXED: Id is now first parameter
                challenge.HostUserId,
                challenge.Title,
                challenge.Description,
                challenge.StartAt,
                challenge.EndAt,
                challenge.Status,
                challenge.MaxParticipants,
                challenge.PrizePool,
                challenge.Category,
                challenge.TaskSummary,
                challenge.CreatedAt,
                challenge.UpdatedAt
            );
        }
        public Task<IEnumerable<ChallengeParticipantDTO>> GetChallengeParticipantsAsync(Guid challengeId)
        {
            return Task.FromResult(Enumerable.Empty<ChallengeParticipantDTO>());
        }

        public Task<IEnumerable<ChallengeRuleDTO>> GetChallengeRulesAsync(Guid challengeId)
        {
            return Task.FromResult(Enumerable.Empty<ChallengeRuleDTO>());
        }

        public Task<IEnumerable<ChallengeTaskDTO>> GetChallengeTasksAsync(Guid challengeId)
        {
            return Task.FromResult(Enumerable.Empty<ChallengeTaskDTO>());
        }

        public Task<ChallengeTaskProgressDTO> GetTaskProgressAsync(Guid taskId)
        {
            return Task.FromResult<ChallengeTaskProgressDTO>(null);
        }

        public Task UpdateTaskProgressAsync(Guid taskId, ChallengeTaskProgressDTO progressDto)
        {
            return Task.CompletedTask;
        }

    }
}
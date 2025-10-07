
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
    public class ChallengeTaskService : IChallengeTaskService
    {
        private readonly IChallengeTaskRepository _challengeTaskRepository;
        private readonly IChallengeRepository _challengeRepository;

        public ChallengeTaskService(IChallengeTaskRepository challengeTaskRepository, IChallengeRepository challengeRepository)
        {
            _challengeTaskRepository = challengeTaskRepository;
            _challengeRepository = challengeRepository;
        }

        public async Task<ChallengeTaskDTO> CreateChallengeTaskAsync(ChallengeTaskDTO challengeTaskDto)
        {
            var challenge = await _challengeRepository.GetByIdAsync(challengeTaskDto.ChallengeId);
            if (challenge == null)
            {
                throw new ArgumentException("Challenge not found.");
            }

            var challengeTask = new ChallengeTask
            {
                ChallengeId = challengeTaskDto.ChallengeId,
                Name = challengeTaskDto.Name,
                Description = challengeTaskDto.Description,
                TaskType = challengeTaskDto.TaskType,
                FrequencyRule = challengeTaskDto.FrequencyRule,
                Points = challengeTaskDto.Points,
                IsRequired = challengeTaskDto.IsRequired,
                ValidationRule = challengeTaskDto.ValidationRule,
                StartDate = challengeTaskDto.StartDate,
                EndDate = challengeTaskDto.EndDate
            };

            var createdChallengeTask = await _challengeTaskRepository.AddAsync(challengeTask);
            return new ChallengeTaskDTO(createdChallengeTask.Id, createdChallengeTask.ChallengeId, createdChallengeTask.Name, createdChallengeTask.Description, createdChallengeTask.TaskType, createdChallengeTask.FrequencyRule, createdChallengeTask.Points, createdChallengeTask.IsRequired, createdChallengeTask.ValidationRule, createdChallengeTask.StartDate, createdChallengeTask.EndDate);
        }

        public async Task<ChallengeTaskDTO> GetChallengeTaskByIdAsync(Guid id)
        {
            var challengeTask = await _challengeTaskRepository.GetByIdAsync(id);
            if (challengeTask == null)
            {
                return null;
            }
            return new ChallengeTaskDTO(challengeTask.Id, challengeTask.ChallengeId, challengeTask.Name, challengeTask.Description, challengeTask.TaskType, challengeTask.FrequencyRule, challengeTask.Points, challengeTask.IsRequired, challengeTask.ValidationRule, challengeTask.StartDate, challengeTask.EndDate);
        }

        public async Task<IEnumerable<ChallengeTaskDTO>> GetAllChallengeTasksAsync()
        {
            var challengeTasks = await _challengeTaskRepository.GetAllAsync();
            return challengeTasks.Select(ct => new ChallengeTaskDTO(ct.Id, ct.ChallengeId, ct.Name, ct.Description, ct.TaskType, ct.FrequencyRule, ct.Points, ct.IsRequired, ct.ValidationRule, ct.StartDate, ct.EndDate));
        }

        public async Task UpdateChallengeTaskAsync(Guid id, ChallengeTaskDTO challengeTaskDto)
        {
            var existingChallengeTask = await _challengeTaskRepository.GetByIdAsync(id);
            if (existingChallengeTask == null)
            {
                throw new ArgumentException("Challenge task not found.");
            }

            if (challengeTaskDto.Name != null)
            {
                existingChallengeTask.Name = challengeTaskDto.Name;
            }
            if (challengeTaskDto.Description != null)
            {
                existingChallengeTask.Description = challengeTaskDto.Description;
            }
            if (challengeTaskDto.TaskType != null)
            {
                existingChallengeTask.TaskType = challengeTaskDto.TaskType;
            }
            if (challengeTaskDto.FrequencyRule != null)
            {
                existingChallengeTask.FrequencyRule = challengeTaskDto.FrequencyRule;
            }
            if (challengeTaskDto.Points.HasValue)
            {
                existingChallengeTask.Points = challengeTaskDto.Points;
            }
            if (challengeTaskDto.IsRequired != existingChallengeTask.IsRequired)
            {
                existingChallengeTask.IsRequired = challengeTaskDto.IsRequired;
            }
            if (challengeTaskDto.ValidationRule != null)
            {
                existingChallengeTask.ValidationRule = challengeTaskDto.ValidationRule;
            }
            if (challengeTaskDto.StartDate.HasValue)
            {
                existingChallengeTask.StartDate = challengeTaskDto.StartDate;
            }
            if (challengeTaskDto.EndDate.HasValue)
            {
                existingChallengeTask.EndDate = challengeTaskDto.EndDate;
            }

            await _challengeTaskRepository.UpdateAsync(existingChallengeTask);
        }

        public async Task DeleteChallengeTaskAsync(Guid id)
        {
            var existingChallengeTask = await _challengeTaskRepository.GetByIdAsync(id);
            if (existingChallengeTask == null)
            {
                throw new ArgumentException("Challenge task not found.");
            }

            await _challengeTaskRepository.DeleteAsync(existingChallengeTask);
        }

        public async Task<IEnumerable<ChallengeTaskDTO>> GetTasksByChallengeIdAsync(Guid challengeId)
        {
            var challengeTasks = await _challengeTaskRepository.GetByChallengeIdAsync(challengeId);
            return challengeTasks.Select(ct => new ChallengeTaskDTO(ct.Id, ct.ChallengeId, ct.Name, ct.Description, ct.TaskType, ct.FrequencyRule, ct.Points, ct.IsRequired, ct.ValidationRule, ct.StartDate, ct.EndDate));
        }
    }
}

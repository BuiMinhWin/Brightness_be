
using ChallengeAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Application.Services
{
    public interface IChallengeTaskService
    {
        Task<ChallengeTaskDTO> CreateChallengeTaskAsync(ChallengeTaskDTO challengeTaskDto);
        Task<ChallengeTaskDTO> GetChallengeTaskByIdAsync(Guid id);
        Task<IEnumerable<ChallengeTaskDTO>> GetAllChallengeTasksAsync();
        Task UpdateChallengeTaskAsync(Guid id, ChallengeTaskDTO challengeTaskDto);
        Task DeleteChallengeTaskAsync(Guid id);
        Task<IEnumerable<ChallengeTaskDTO>> GetTasksByChallengeIdAsync(Guid challengeId);
    }
}

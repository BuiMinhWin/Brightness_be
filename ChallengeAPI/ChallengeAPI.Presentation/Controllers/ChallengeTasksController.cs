
using ChallengeAPI.Application.DTOs;
using ChallengeAPI.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChallengeAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChallengeTasksController : ControllerBase
    {
        private readonly IChallengeTaskService _challengeTaskService;

        public ChallengeTasksController(IChallengeTaskService challengeTaskService)
        {
            _challengeTaskService = challengeTaskService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChallengeTask([FromBody] ChallengeTaskDTO challengeTaskDto)
        {
            try
            {
                var createdChallengeTask = await _challengeTaskService.CreateChallengeTaskAsync(challengeTaskDto);
                return CreatedAtAction(nameof(GetChallengeTask), new { id = createdChallengeTask.Id }, createdChallengeTask);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChallengeTask(Guid id)
        {
            var challengeTask = await _challengeTaskService.GetChallengeTaskByIdAsync(id);
            if (challengeTask == null)
            {
                return NotFound();
            }
            return Ok(challengeTask);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChallengeTasks()
        {
            var challengeTasks = await _challengeTaskService.GetAllChallengeTasksAsync();
            return Ok(challengeTasks);
        }

        [HttpGet("/api/challenges/{challengeId}/tasks")]
        public async Task<IActionResult> GetTasksByChallengeId(Guid challengeId)
        {
            var challengeTasks = await _challengeTaskService.GetTasksByChallengeIdAsync(challengeId);
            return Ok(challengeTasks);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChallengeTask(Guid id, [FromBody] ChallengeTaskDTO challengeTaskDto)
        {
            try
            {
                await _challengeTaskService.UpdateChallengeTaskAsync(id, challengeTaskDto);
                return Ok(new { message = "Updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallengeTask(Guid id)
        { 
            try
            {
                await _challengeTaskService.DeleteChallengeTaskAsync(id);
                return Ok($"Challenge task {id} deleted");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

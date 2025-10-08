
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
    public class ChallengeTaskProgressesController : ControllerBase
    {
        private readonly IChallengeTaskProgressService _challengeTaskProgressService;

        public ChallengeTaskProgressesController(IChallengeTaskProgressService challengeTaskProgressService)
        {
            _challengeTaskProgressService = challengeTaskProgressService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChallengeTaskProgress([FromBody] ChallengeTaskProgressDTO challengeTaskProgressDto)
        {
            try
            {
                var createdChallengeTaskProgress = await _challengeTaskProgressService.CreateChallengeTaskProgressAsync(challengeTaskProgressDto);
                return CreatedAtAction(nameof(GetChallengeTaskProgress), new { id = createdChallengeTaskProgress.Id }, createdChallengeTaskProgress);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChallengeTaskProgress(Guid id)
        {
            var challengeTaskProgress = await _challengeTaskProgressService.GetChallengeTaskProgressByIdAsync(id);
            if (challengeTaskProgress == null)
            {
                return NotFound();
            }
            return Ok(challengeTaskProgress);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChallengeTaskProgresses()
        {
            var challengeTaskProgresses = await _challengeTaskProgressService.GetAllChallengeTaskProgressesAsync();
            return Ok(challengeTaskProgresses);
        }

        [HttpGet("/api/participants/{participantId}/progresses")]
        public async Task<IActionResult> GetProgressByParticipantId(Guid participantId)
        {
            var challengeTaskProgresses = await _challengeTaskProgressService.GetProgressByParticipantIdAsync(participantId);
            return Ok(challengeTaskProgresses);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChallengeTaskProgress(Guid id, [FromBody] ChallengeTaskProgressDTO challengeTaskProgressDto)
        {
            try
            {
                await _challengeTaskProgressService.UpdateChallengeTaskProgressAsync(id, challengeTaskProgressDto);
                return Ok(new { message = "Updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchChallengeTaskProgress(Guid id, [FromBody] Dictionary<string, object> patchData)
        {
            try
            {
                await _challengeTaskProgressService.PatchChallengeTaskProgressAsync(id, patchData);
                return Ok(new { message = "Updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallengeTaskProgress(Guid id)
        { 
            try
            {
                await _challengeTaskProgressService.DeleteChallengeTaskProgressAsync(id);
                return Ok($"Challenge task progress {id} deleted");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

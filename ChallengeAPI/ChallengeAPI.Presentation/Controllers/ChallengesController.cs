
using ChallengeAPI.Application.DTOs;
using ChallengeAPI.Application.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChallengesController : ControllerBase
    {
        private readonly IChallengeService _challengeService;

        public ChallengesController(IChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChallenge([FromBody] ChallengeDTO challengeDto)
        {
            try
            {
                var createdChallenge = await _challengeService.CreateChallengeAsync(challengeDto);
                var createdChallengeDto = new ChallengeDTO(
                    createdChallenge.Id,
                    createdChallenge.HostUserId,
                    createdChallenge.Title,
                    createdChallenge.Description,
                    createdChallenge.StartAt,
                    createdChallenge.EndAt,
                    createdChallenge.Status,
                    createdChallenge.MaxParticipants,
                    createdChallenge.PrizePool,
                    createdChallenge.Category,
                    createdChallenge.TaskSummary,
                    createdChallenge.CreatedAt,
                    createdChallenge.UpdatedAt
                );
                return CreatedAtAction(nameof(GetChallenge), new { id = createdChallenge.Id }, createdChallengeDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }





        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChallenge(Guid id, [FromBody] ChallengeDTO challengeDto)
        {
            try
            {
                await _challengeService.UpdateChallengeAsync(id, challengeDto);
                return Ok(new { message = "Updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }


        // PATCH - Partial update (update only specified fields)
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchChallenge(Guid id, [FromBody] Dictionary<string, object> patchData)
        {
            try
            {
                await _challengeService.PatchChallengeAsync(id, patchData);
                return Ok(new { message = "Challenge rule updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallenge(Guid id)
        {
            await _challengeService.DeleteChallengeAsync(id);
            return Ok($"Challenge {id} deleted");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChallenge(Guid id)
        {
            var challenge = await _challengeService.GetChallengeByIdAsync(id);
            if (challenge == null)
            {
                return NotFound();
            }
            return Ok(challenge);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChallenges()
        {
            var challenges = await _challengeService.GetAllChallengesAsync();
            return Ok(challenges);
        }
    }
}


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
    public class ChallengeParticipantsController : ControllerBase
    {
        private readonly IChallengeParticipantService _challengeParticipantService;

        public ChallengeParticipantsController(IChallengeParticipantService challengeParticipantService)
        {
            _challengeParticipantService = challengeParticipantService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChallengeParticipant([FromBody] ChallengeParticipantDTO challengeParticipantDto)
        {
            try
            {
                var createdChallengeParticipant = await _challengeParticipantService.CreateChallengeParticipantAsync(challengeParticipantDto);
                return CreatedAtAction(nameof(GetChallengeParticipant), new { id = createdChallengeParticipant.Id }, createdChallengeParticipant);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChallengeParticipant(Guid id)
        {
            var challengeParticipant = await _challengeParticipantService.GetChallengeParticipantByIdAsync(id);
            if (challengeParticipant == null)
            {
                return NotFound();
            }
            return Ok(challengeParticipant);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChallengeParticipants()
        {
            var challengeParticipants = await _challengeParticipantService.GetAllChallengeParticipantsAsync();
            return Ok(challengeParticipants);
        }

        [HttpGet("/api/challenges/{challengeId}/participants")]
        public async Task<IActionResult> GetParticipantsByChallengeId(Guid challengeId)
        {
            var challengeParticipants = await _challengeParticipantService.GetParticipantsByChallengeIdAsync(challengeId);
            return Ok(challengeParticipants);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChallengeParticipant(Guid id, [FromBody] ChallengeParticipantDTO challengeParticipantDto)
        {
            try
            {
                await _challengeParticipantService.UpdateChallengeParticipantAsync(id, challengeParticipantDto);
                return Ok(new { message = "Updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallengeParticipant(Guid id)
        { 
            try
            {
                await _challengeParticipantService.DeleteChallengeParticipantAsync(id);
                return Ok($"Challenge participant {id} deleted");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

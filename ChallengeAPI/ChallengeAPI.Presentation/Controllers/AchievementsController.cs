
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
    public class AchievementsController : ControllerBase
    {
        private readonly IAchievementService _achievementService;

        public AchievementsController(IAchievementService achievementService)
        {
            _achievementService = achievementService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAchievement([FromBody] AchievementDTO achievementDto)
        {
            try
            {
                var createdAchievement = await _achievementService.CreateAchievementAsync(achievementDto);
                return CreatedAtAction(nameof(GetAchievement), new { id = createdAchievement.Id }, createdAchievement);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAchievement(Guid id)
        {
            var achievement = await _achievementService.GetAchievementByIdAsync(id);
            if (achievement == null)
            {
                return NotFound();
            }
            return Ok(achievement);
        }


        [HttpGet("/api/users/{userId?}/achievements")]
        public async Task<IActionResult> GetAchievementsByUserId(Guid? userId)
        {
            if (userId.HasValue)
            {
                var achievements = await _achievementService.GetAchievementsByUserIdAsync(userId.Value);
                return Ok(achievements);
            }
            else
            {
                var achievements = await _achievementService.GetAllAchievementsAsync();
                return Ok(achievements);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAchievement(Guid id, [FromBody] AchievementDTO achievementDto)
        {
            try
            {
                await _achievementService.UpdateAchievementAsync(id, achievementDto);
                return Ok(new { message = "Updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAchievement(Guid id)
        { 
            try
            {
                await _achievementService.DeleteAchievementAsync(id);
                return Ok($"Achievement {id} deleted");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

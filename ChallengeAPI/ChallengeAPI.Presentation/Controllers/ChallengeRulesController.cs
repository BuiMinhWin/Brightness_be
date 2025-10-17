
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
    public class ChallengeRulesController : ControllerBase
    {
        private readonly IChallengeRuleService _challengeRuleService;

        public ChallengeRulesController(IChallengeRuleService challengeRuleService)
        {
            _challengeRuleService = challengeRuleService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChallengeRule(ChallengeRuleDTO challengeRuleDto)
        {
            try
            {
                var createdChallengeRule = await _challengeRuleService.CreateChallengeRuleAsync(challengeRuleDto);
                return CreatedAtAction(nameof(GetChallengeRule), new { id = createdChallengeRule.Id }, createdChallengeRule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChallengeRule(Guid id)
        {
            var challengeRule = await _challengeRuleService.GetChallengeRuleByIdAsync(id);
            if (challengeRule == null)
            {
                return NotFound();
            }
            return Ok(challengeRule);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChallengeRules()
        {
            var challengeRules = await _challengeRuleService.GetAllChallengeRulesAsync();
            return Ok(challengeRules);
        }

        [HttpGet("/api/challenges/{challengeId}/rules")]
        public async Task<IActionResult> GetRulesByChallengeId(Guid challengeId)
        {
            var challengeRules = await _challengeRuleService.GetRulesByChallengeIdAsync(challengeId);
            return Ok(challengeRules);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChallengeRule(Guid id, [FromBody] ChallengeRuleDTO challengeRuleDto)
        {
            try
            {
                await _challengeRuleService.UpdateChallengeRuleAsync(id, challengeRuleDto);
                return Ok(new { message = "Challenge rule updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchChallengeRule(Guid id, [FromBody] Dictionary<string, object> patchData)
        {
            try
            {
                await _challengeRuleService.PatchChallengeRuleAsync(id, patchData);
                return Ok(new { message = "Challenge rule updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChallengeRule(Guid id)
        {
            try
            {
                await _challengeRuleService.DeleteChallengeRuleAsync(id);
                return Ok($"Challenge rule {id} deleted");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}


using ChallengeAPI.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ChallengeAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLeaderboards()
        {
            var leaderboards = await _leaderboardService.GetAllLeaderboardsAsync();
            return Ok(leaderboards);
        }

        [HttpGet("{challengeId}")]
        public async Task<IActionResult> GetLeaderboard(Guid challengeId)
        {
            var leaderboard = await _leaderboardService.GetLeaderboardAsync(challengeId);
            return Ok(leaderboard);
        }

        [HttpGet("{challengeId}/users/{userId}")]
        public async Task<IActionResult> GetUserRank(Guid challengeId, Guid userId)
        {
            var rank = await _leaderboardService.GetUserRankAsync(challengeId, userId);
            if (rank == null)
            {
                return NotFound();
            }
            return Ok(rank);
        }

        [HttpDelete("{challengeId}")]
        public async Task<IActionResult> ResetLeaderboard(Guid challengeId)
        {
            await _leaderboardService.ResetLeaderboardAsync(challengeId);
            return NoContent();
        }
    }
}

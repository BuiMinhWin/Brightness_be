using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using System.Security.Claims;

namespace PersonalCoachAPI.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(
            IUserProfileService userProfileService,
            ILogger<UserProfileController> logger)
        {
            _userProfileService = userProfileService;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserProfileResponse>> GetUserProfile(Guid userId)
        {
            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(userId);
                return Ok(profile);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserProfileResponse>> CreateOrUpdateProfile(
            [FromBody] CreateUserProfileRequest request)
        {
            try
            {
                // Validate user from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tokenUserId))
                {
                    return Unauthorized("Invalid token");
                }

                // Ensure user can only update their own profile
                if (request.UserId != tokenUserId)
                {
                    return Forbid("You can only update your own profile");
                }

                var profile = await _userProfileService.CreateOrUpdateProfileAsync(request);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating profile for user {UserId}", request.UserId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}


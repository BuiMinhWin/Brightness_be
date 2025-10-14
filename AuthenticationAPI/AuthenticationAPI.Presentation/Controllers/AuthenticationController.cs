using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationAPI.Application.DTOs;
using AuthenticationAPI.Application.Interfaces;
using BrightNess.Library.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAPI.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(IUserService userService) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<Response>> Register(UserDTO userDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await userService.RegisterAsync(userDTO);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<Response>> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ipAddress = GetClientIpAddress();
            var result = await userService.LoginAsync(loginDTO, ipAddress);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<Response>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await userService.RefreshTokenAsync(request.RefreshToken, GetClientIpAddress());
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<ActionResult<Response>> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await userService.RevokeRefreshTokenAsync(request.RefreshToken, GetClientIpAddress());
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<GetUserDTO>> GetUser(Guid id)
        {
            var user = await userService.GetByIdAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GetUserDTO>>> GetUsers()
        {
            var users = await userService.GetAllAsync();
            var userList = users?.ToList();

            if (userList is null || userList.Count == 0)
            {
                return NotFound();
            }

            return Ok(userList);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<Response>> Update(Guid id, UpdateUserDTO updateUserDto)
        {
            var result = await userService.UpdateAsync(id, updateUserDto);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id:guid}/subscription")]
        [AllowAnonymous] // Allow BillingAPI to call without user token
        public async Task<ActionResult<Response>> UpdateSubscription(Guid id, [FromBody] UpdateSubscriptionRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await userService.UpdateSubscriptionAsync(id, request.SubscriptionPlanId);
            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<Response>> Delete(Guid id)
        {
            var result = await userService.DeleteAsync(id);
            return result.Flag ? Ok(result) : NotFound(result);
        }

        private string? GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}


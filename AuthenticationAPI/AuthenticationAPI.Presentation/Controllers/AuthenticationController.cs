using System;
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
    [AllowAnonymous]
    public class AuthenticationController(IUser userInterface) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<Response>> Register(UserDTO userDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await userInterface.Register(userDTO);

            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<Response>> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await userInterface.Login(loginDTO);

            return result.Flag ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<GetUserDTO>> GetUser(Guid id)
        {
            var user = await userInterface.GetUser(id);
            return user is null ? NotFound() : Ok(user);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/comments/{commentId}/vote")]
    public class CommentVotesController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentVotesController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> VoteComment(Guid commentId, [FromBody] CommentVoteDTO voteDto)
        {
            //var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
            await _commentService.VoteCommentAsync(commentId, userId, voteDto.VoteType);
            return Ok("React success");
        }

        //[Authorize]
        [HttpDelete]
        public async Task<IActionResult> CancelCommentVote(Guid commentId)
        {
            //var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
            await _commentService.CancelCommentVoteAsync(commentId, userId);
            return Ok("Cancel success");
        }
    }
}

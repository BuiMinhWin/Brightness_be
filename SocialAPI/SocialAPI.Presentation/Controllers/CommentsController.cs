using Microsoft.AspNetCore.Mvc;
using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/posts/{postId}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments(Guid postId)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult<CommentDTO>> CreateComment(Guid postId, CreateCommentDTO createCommentDto)
        {
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            //if (userIdClaim == null)
            //{
            //    return Unauthorized("User not authenticated.");
            //}
            //var userId = Guid.Parse(userIdClaim.Value);
            var userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"); // dùng để test tạm thời
            var comment = await _commentService.CreateCommentAsync(postId, createCommentDto, userId);
            return CreatedAtAction(nameof(GetComment), new { postId = postId, commentId = comment.Id }, comment);
        }

        [HttpGet("{commentId}")]
        public async Task<ActionResult<CommentDTO>> GetComment(Guid postId, Guid commentId)
        {
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null || comment.PostId != postId)
            {
                return NotFound();
            }
            return Ok(comment);
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid postId, Guid commentId, [FromBody] UpdateCommentDTO updateCommentDto)
        {
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            //if (userIdClaim == null)
            //{
            //    return Unauthorized("User not authenticated.");
            //}
            //var userId = Guid.Parse(userIdClaim.Value);
            var userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"); // dùng để test tạm thời
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null || comment.PostId != postId)
            {
                return NotFound();
            }
            await _commentService.UpdateCommentAsync(commentId, updateCommentDto, userId);
            return Ok(updateCommentDto);
        }

        [HttpPatch("{commentId}/visibility")]
        public async Task<IActionResult> ToggleCommentVisibility(Guid postId, Guid commentId)
        {
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            //if (userIdClaim == null)
            //{
            //    return Unauthorized("User not authenticated.");
            //}
            //var userId = Guid.Parse(userIdClaim.Value);
            var userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"); // dùng để test tạm thời
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null || comment.PostId != postId)
            {
                return NotFound();
            }
            await _commentService.ToggleCommentVisibilityAsync(commentId, userId);
            return Ok(new { message = "Success" });
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid postId, Guid commentId)
        {
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            //if (userIdClaim == null)
            //{
            //    return Unauthorized("User not authenticated.");
            //}
            //var userId = Guid.Parse(userIdClaim.Value);
            var userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"); // dùng để test tạm thời
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null || comment.PostId != postId)
            {
                return NotFound();
            }
            await _commentService.DeleteCommentAsync(commentId, userId);
            return NoContent();
        }
    }
}

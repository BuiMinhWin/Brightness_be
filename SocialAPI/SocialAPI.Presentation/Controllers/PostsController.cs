using Microsoft.AspNetCore.Mvc;
using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SocialAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDTO>>> GetPosts()
        {
            var posts = await _postService.GetPostsAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostDTO>> GetPostById(Guid id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpPost]
        public async Task<ActionResult<PostDTO>> CreatePost(PostDTO postDto)
        {
            var createdPost = await _postService.CreatePostAsync(postDto);
            return CreatedAtAction(nameof(GetPostById), new { id = createdPost.Id }, createdPost);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] PostDTO postDto)
        {
            if (id != postDto.Id)
                return BadRequest("ID in URL and body do not match.");

            var updatedPost = await _postService.UpdatePostAsync(id, postDto);
            return Ok(updatedPost);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdatePostStatus(Guid id, [FromBody] string status)
        {
            var updatedPost = await _postService.UpdatePostStatusAsync(id, status);
            return Ok(updatedPost);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            await _postService.DeletePostAsync(id);
            return Ok("Delete success");
        }

        [HttpGet("tag/{tagSlug}")]
        public async Task<ActionResult<IEnumerable<PostDTO>>> GetPostsByTag(string tagSlug)
        {
            var posts = await _postService.GetPostsByTagAsync(tagSlug);
            return Ok(posts);
        }

        //[Authorize]
        [HttpPost("{id}/vote")]
        public async Task<IActionResult> VotePost(Guid id, [FromBody] PostVoteDTO voteDto)
        {
            //var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"); // ví dụ 1 user giả
            await _postService.VotePostAsync(id, userId, voteDto.VoteType);
            return Ok("Vote success");
        }

        //[Authorize]
        [HttpDelete("{id}/vote")]
        public async Task<IActionResult> CancelPostVote(Guid id)
        {
            //var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"); // ví dụ 1 user giả
            await _postService.CancelPostVoteAsync(id, userId);
            return Ok("Cancel Success");
        }
    }
}

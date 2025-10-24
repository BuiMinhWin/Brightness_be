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
        private readonly IReportService _reportService;

        public PostsController(IPostService postService, IReportService reportService)
        {
            _postService = postService;
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDTO>>> GetPosts()
        {
            // Try to get userId from JWT claims (optional - for anonymous browsing support)
            Guid? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }
            
            var posts = await _postService.GetPostsAsync(userId);
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PostDTO>> GetPostById(Guid id)
        {
            // Try to get userId from JWT claims (optional - for anonymous browsing support)
            Guid? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
            {
                userId = parsedUserId;
            }
            
            var post = await _postService.GetPostByIdAsync(id, userId);
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

        [Authorize]
        [HttpPost("{id}/vote")]
        public async Task<IActionResult> VotePost(Guid id, [FromBody] PostVoteDTO voteDto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _postService.VotePostAsync(id, userId, voteDto.VoteType);
            return Ok("Vote success");
        }

        [Authorize]
        [HttpDelete("{id}/vote")]
        public async Task<IActionResult> CancelPostVote(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _postService.CancelPostVoteAsync(id, userId);
            return Ok("Cancel Success");
        }

        [Authorize]
        [HttpGet("{id}/vote")]
        public async Task<IActionResult> GetUserVote(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var vote = await _postService.GetUserVoteAsync(id, userId);
            if (vote == null)
            {
                return Ok(new { hasVoted = false, voteType = (string)null });
            }
            return Ok(new { hasVoted = true, voteType = vote.VoteType });
        }

        // Report endpoints
        [Authorize]
        [HttpPost("{id}/report")]
        public async Task<IActionResult> ReportPost(Guid id, [FromBody] ReportRequestDTO reportRequest)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            
            try
            {
                var createReportDto = new CreateReportDTO("post", id, reportRequest.Reason);
                await _reportService.CreateReportAsync(createReportDto, userId);
                
                return Ok("Report submitted successfully");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet("flagged")]
        public async Task<ActionResult<IEnumerable<PostDTO>>> GetFlaggedPosts([FromQuery] int minReportCount = 5)
        {
            var flaggedPosts = await _postService.GetFlaggedPostsAsync(minReportCount);
            return Ok(flaggedPosts);
        }

        //[Authorize(Roles = "Admin")]
        [HttpDelete("{id}/soft-delete")]
        public async Task<IActionResult> SoftDeletePost(Guid id)
        {
            await _postService.SoftDeletePostAsync(id);
            return Ok("Post soft deleted successfully");
        }

        [HttpGet("{id}/reports/count")]
        public async Task<IActionResult> GetReportCount(Guid id)
        {
            var count = await _postService.GetReportCountAsync(id);
            return Ok(new { reportCount = count });
        }
    }
}

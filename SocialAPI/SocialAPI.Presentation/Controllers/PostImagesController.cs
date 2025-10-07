using Microsoft.AspNetCore.Mvc;
using SocialAPI.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace SocialAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostImagesController : ControllerBase
    {
        private readonly IPostImageService _postImageService;

        public PostImagesController(IPostImageService postImageService)
        {
            _postImageService = postImageService;
        }

        [HttpPost("{postId}/images")]
        public async Task<IActionResult> UploadImage(Guid postId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                var imageDto = await _postImageService.UploadImageAsync(postId, file);
                return CreatedAtAction(nameof(GetImage), new { imageId = imageDto.Id }, imageDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{postId}/images")]
        public async Task<IActionResult> GetPostImages(Guid postId)
        {
            var images = await _postImageService.GetPostImagesAsync(postId);
            return Ok(images);
        }

        [HttpGet("images/{imageId}")]
        public async Task<IActionResult> GetImage(Guid imageId)
        {
            var image = await _postImageService.GetPostImageByIdAsync(imageId);
            if (image == null)
            {
                return NotFound();
            }
            return Ok(image);
        }

        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(Guid imageId)
        {
            try
            {
                await _postImageService.DeletePostImageAsync(imageId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
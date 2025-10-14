using MediaAPI.Application.DTOs;
using MediaAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace MediaAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/media")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

      
        [HttpGet]
        public async Task<IActionResult> GetAllVideos(
            int pageNumber = 1, 
            int pageSize = 10,
            string? search = null,
            string? sortBy = null)
        {
            var videos = await _mediaService.GetAllVideosAsync(pageNumber, pageSize, search, sortBy);
            return Ok(videos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVideoById(Guid id)
        {
            var video = await _mediaService.GetVideoByIdAsync(id);
            if (video == null)
                return NotFound("Video not found.");

            return Ok(video);
        }

        // ➕ Tạo video mới
        [HttpPost]
        public async Task<IActionResult> CreateVideo([FromBody] CreateMediaItemRequest createRequest)
        {
            var newVideo = await _mediaService.CreateVideoAsync(createRequest);
            return CreatedAtAction(nameof(GetVideoById), new { id = newVideo.Id }, newVideo);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVideo(Guid id, [FromBody] UpdateMediaItemRequest updateRequest)
        {
            var result = await _mediaService.UpdateVideoAsync(id, updateRequest);
            if (!result)
                return NotFound("Video not found.");

            return Ok("Update success");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideo(Guid id)
        {
            var result = await _mediaService.DeleteVideoAsync(id);
            if (!result)
                return NotFound("Video not found.");

            return Ok("Delete success");
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeVideo(Guid id)
        {
            
            var userId = Guid.NewGuid();
            var result = await _mediaService.LikeVideoAsync(id, userId);
            if (!result)
                return NotFound("Video not found.");

            return Ok("Like success");
        }

        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikeVideo(Guid id)
        {
            var userId = Guid.NewGuid();
            var result = await _mediaService.UnlikeVideoAsync(id, userId);
            if (!result)
                return NotFound("Video not found.");

            return Ok("Unlike success");
        }

       
        [HttpPost("{id}/view")]
        public async Task<IActionResult> ViewVideo(Guid id)
        {
            var userId = Guid.NewGuid();
            var result = await _mediaService.ViewVideoAsync(id, userId);
            if (!result)
                return NotFound("Video not found.");

            return Ok("View success");
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadVideo(Guid id)
        {
            var video = await _mediaService.GetVideoByIdAsync(id);
            if (video == null)
                return NotFound();

            var stream = await _mediaService.GetVideoFileAsync(id);

            return new FileStreamResult(stream, "video/mp4")
            {
                FileDownloadName = $"{video.Title?.Replace(' ', '_')}.mp4"
            };
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/tags")]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagsController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDTO>>> GetTags()
        {
            var tags = await _tagService.GetTagsAsync();
            return Ok(tags);
        }

        [HttpPost]
        public async Task<ActionResult<TagDTO>> CreateTag(CreateTagDTO createTagDto)
        {
            var tag = await _tagService.CreateTagAsync(createTagDto);
            return CreatedAtAction(nameof(GetTag), new { tagId = tag.Id }, tag);
        }

        [HttpGet("{tagId}")]
        public async Task<ActionResult<TagDTO>> GetTag(Guid tagId)
        {
            var tag = await _tagService.GetTagByIdAsync(tagId);
            if (tag == null)
            {
                return NotFound();
            }
            return Ok(tag);
        }

        [HttpDelete("{tagId}")]
        public async Task<IActionResult> DeleteTag(Guid tagId)
        {
            await _tagService.DeleteTagAsync(tagId);
            return NoContent();
        }
    }
}

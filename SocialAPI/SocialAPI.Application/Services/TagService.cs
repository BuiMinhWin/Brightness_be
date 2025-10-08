using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using SocialAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialAPI.Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<TagDTO>> GetTagsAsync()
        {
            var tags = await _tagRepository.GetTagsAsync();
            return tags.Select(t => new TagDTO(t.Id, t.Name, t.Slug, t.CreatedAt));
        }

        public async Task<TagDTO> GetTagByIdAsync(Guid tagId)
        {
            var tag = await _tagRepository.GetTagByIdAsync(tagId);
            if (tag == null) return null;
            return new TagDTO(tag.Id, tag.Name, tag.Slug, tag.CreatedAt);
        }

        public async Task<TagDTO> CreateTagAsync(CreateTagDTO tagDto)
        {
            var tag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = tagDto.Name,
                Slug = tagDto.Slug,
                CreatedAt = DateTime.UtcNow
            };

            var createdTag = await _tagRepository.CreateTagAsync(tag);
            return new TagDTO(createdTag.Id, createdTag.Name, createdTag.Slug, createdTag.CreatedAt);
        }

        public async Task DeleteTagAsync(Guid tagId)
        {
            await _tagRepository.DeleteTagAsync(tagId);
        }
    }
}

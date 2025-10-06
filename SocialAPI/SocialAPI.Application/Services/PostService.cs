using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using SocialAPI.Domain.Entities;

namespace SocialAPI.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<PostDTO> CreatePostAsync(PostDTO postDto)
        {
            var post = new Post
            {
                Id = Guid.NewGuid(),
                UserId = postDto.UserId,
                Title = postDto.Title,
                Content = postDto.Content,
                Visibility = postDto.Visibility,
                Status = postDto.Status,
                CreatedAt = DateTime.UtcNow
            };

            var createdPost = await _postRepository.CreatePostAsync(post);
            return new PostDTO(createdPost.Id, createdPost.UserId, createdPost.Title, createdPost.Content, createdPost.Visibility, createdPost.Status, createdPost.CreatedAt, createdPost.UpdatedAt, 0);
        }

        public async Task DeletePostAsync(Guid id)
        {
            await _postRepository.DeletePostAsync(id);
        }

        public async Task<PostDTO> GetPostByIdAsync(Guid id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null) return null;
            return new PostDTO(post.Id, post.UserId, post.Title, post.Content, post.Visibility, post.Status, post.CreatedAt, post.UpdatedAt, await CalculateReactions(post.Id));
        }

        public async Task<IEnumerable<PostDTO>> GetPostsAsync()
        {
            var posts = await _postRepository.GetPostsAsync();
            var postDtos = new List<PostDTO>();
            foreach (var post in posts)
            {
                postDtos.Add(new PostDTO(post.Id, post.UserId, post.Title, post.Content, post.Visibility, post.Status, post.CreatedAt, post.UpdatedAt, await CalculateReactions(post.Id)));
            }
            return postDtos;
        }

        public async Task<PostDTO> UpdatePostAsync(Guid id, PostDTO postDto)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null) throw new Exception("Post not found");

            post.Title = postDto.Title;
            post.Content = postDto.Content;
            post.Visibility = postDto.Visibility;
            post.Status = postDto.Status;
            post.UpdatedAt = DateTime.UtcNow;

            await _postRepository.UpdatePostAsync(post);

            // Sử dụng positional constructor
            return new PostDTO(
                post.Id,
                post.UserId,
                post.Title,
                post.Content,
                post.Visibility,
                post.Status,
                post.CreatedAt,
                post.UpdatedAt,
                await CalculateReactions(post.Id)
            );
        }


        public async Task<PostDTO> UpdatePostStatusAsync(Guid id, string status)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
                throw new Exception("Post not found");

            post.Status = status;
            post.UpdatedAt = DateTime.UtcNow;

            await _postRepository.UpdatePostAsync(post);

            // Trả lại thông tin bài post đã cập nhật
            return new PostDTO(
                post.Id,
                post.UserId,
                post.Title,
                post.Content,
                post.Visibility,
                post.Status,
                post.CreatedAt,
                post.UpdatedAt,
                await CalculateReactions(post.Id)
            );
        }

        public async Task VotePostAsync(Guid postId, Guid userId, string voteType)
        {
            var existingVote = await _postRepository.GetPostVoteAsync(postId, userId);
            if (existingVote != null)
            {
                if (existingVote.VoteType == voteType)
                {
                    // User is casting the same vote again, so we cancel it
                    await _postRepository.RemovePostVoteAsync(existingVote);
                }
                else
                {
                    // User is changing their vote
                    existingVote.VoteType = voteType;
                    await _postRepository.UpdatePostVoteAsync(existingVote);
                }
            }
            else
            {
                // New vote
                var newVote = new PostVote
                {
                    Id = Guid.NewGuid(),
                    PostId = postId,
                    UserId = userId,
                    VoteType = voteType,
                    CreatedAt = DateTime.UtcNow
                };
                await _postRepository.AddPostVoteAsync(newVote);
            }
        }

        public async Task CancelPostVoteAsync(Guid postId, Guid userId)
        {
            var existingVote = await _postRepository.GetPostVoteAsync(postId, userId);
            if (existingVote != null)
            {
                await _postRepository.RemovePostVoteAsync(existingVote);
            }
        }

        private async Task<int> CalculateReactions(Guid postId)
        {
            var votes = await _postRepository.GetVotesForPostAsync(postId);
            return votes.Count(v => v.VoteType == "upvote") - votes.Count(v => v.VoteType == "downvote");
        }

        public async Task<IEnumerable<PostDTO>> GetPostsByTagAsync(string tagSlug)
        {
            var posts = await _postRepository.GetPostsByTagAsync(tagSlug);
            var postDtos = new List<PostDTO>();
            foreach (var post in posts)
            {
                postDtos.Add(new PostDTO(post.Id, post.UserId, post.Title, post.Content, post.Visibility, post.Status, post.CreatedAt, post.UpdatedAt, await CalculateReactions(post.Id)));
            }
            return postDtos;
        }
    }
}

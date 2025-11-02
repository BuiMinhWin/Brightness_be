using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using SocialAPI.Domain.Entities;
using System.Linq;

namespace SocialAPI.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IReportRepository _reportRepository;

        public PostService(IPostRepository postRepository, ICommentRepository commentRepository, IReportRepository reportRepository)
        {
            _postRepository = postRepository;
            _commentRepository = commentRepository;
            _reportRepository = reportRepository;
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
            return new PostDTO(createdPost.Id, createdPost.UserId, createdPost.Title, createdPost.Content, createdPost.Visibility, createdPost.Status, createdPost.CreatedAt, createdPost.UpdatedAt, 0, 0, 0);
        }

        public async Task DeletePostAsync(Guid id)
        {
            await _postRepository.DeletePostAsync(id);
        }

        public async Task<PostDTO> GetPostByIdAsync(Guid id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null) return null;
            var (reactionCount, dislikeCount) = await CalculateReactionsAndDislikes(post.Id);
            var commentCount = await CalculateCommentCount(post.Id);
            return new PostDTO(post.Id, post.UserId, post.Title, post.Content, post.Visibility, post.Status, post.CreatedAt, post.UpdatedAt, reactionCount, dislikeCount, commentCount);
        }

        public async Task<IEnumerable<PostDTO>> GetPostsAsync()
        {
            var posts = await _postRepository.GetPostsAsync();
            var postDtos = new List<PostDTO>();
            foreach (var post in posts)
            {
                // Double-check to exclude deleted posts (in case repository filter didn't catch it)
                if (post.Status != null && post.Status.ToLowerInvariant() == "deleted")
                {
                    continue;
                }
                
                var (reactionCount, dislikeCount) = await CalculateReactionsAndDislikes(post.Id);
                var commentCount = await CalculateCommentCount(post.Id);
                postDtos.Add(new PostDTO(post.Id, post.UserId, post.Title, post.Content, post.Visibility, post.Status, post.CreatedAt, post.UpdatedAt, reactionCount, dislikeCount, commentCount));
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

            var (reactionCount, dislikeCount) = await CalculateReactionsAndDislikes(post.Id);
            var commentCount = await CalculateCommentCount(post.Id);
            return new PostDTO(
                post.Id,
                post.UserId,
                post.Title,
                post.Content,
                post.Visibility,
                post.Status,
                post.CreatedAt,
                post.UpdatedAt,
                reactionCount,
                dislikeCount,
                commentCount
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

            var (reactionCount, dislikeCount) = await CalculateReactionsAndDislikes(post.Id);
            var commentCount = await CalculateCommentCount(post.Id);

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
                reactionCount,
                dislikeCount,
                commentCount
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

            await UpdatePostReactionCountAsync(postId);
        }

        public async Task CancelPostVoteAsync(Guid postId, Guid userId)
        {
            var existingVote = await _postRepository.GetPostVoteAsync(postId, userId);
            if (existingVote != null)
            {
                await _postRepository.RemovePostVoteAsync(existingVote);
            }

            await UpdatePostReactionCountAsync(postId);
        }

        public async Task<PostVoteDTO> GetUserVoteAsync(Guid postId, Guid userId)
        {
            var vote = await _postRepository.GetPostVoteAsync(postId, userId);
            if (vote == null) return null;
            
            return new PostVoteDTO(vote.Id, vote.PostId, vote.UserId, vote.VoteType, vote.CreatedAt);
        }

        public async Task UpdatePostReactionCountAsync(Guid postId)
        {
            var (reactionCount, dislikeCount) = await CalculateReactionsAndDislikes(postId);
            await _postRepository.UpdatePostCountsAsync(postId, reactionCount, dislikeCount);
        }

        private async Task<(int, int)> CalculateReactionsAndDislikes(Guid postId)
        {
            var postVotes = await _postRepository.GetVotesForPostAsync(postId);
            var postReactionCount = postVotes.Count(v => v.VoteType == "reaction");
            var postDislikeCount = postVotes.Count(v => v.VoteType == "dislike");

            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
            var commentReactionCount = comments.Sum(c => c.ReactionCount);

            return (postReactionCount, postDislikeCount);
        }

        public async Task<IEnumerable<PostDTO>> GetPostsByTagAsync(string tagSlug)
        {
            var posts = await _postRepository.GetPostsByTagAsync(tagSlug);
            var postDtos = new List<PostDTO>();
            foreach (var post in posts)
            {
                var (reactionCount, dislikeCount) = await CalculateReactionsAndDislikes(post.Id);
                var commentCount = await CalculateCommentCount(post.Id);
                postDtos.Add(new PostDTO(post.Id, post.UserId, post.Title, post.Content, post.Visibility, post.Status, post.CreatedAt, post.UpdatedAt, reactionCount, dislikeCount, commentCount));
            }
            return postDtos;
        }

        private async Task<int> CalculateCommentCount(Guid postId)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);
            return comments.Count();
        }

        public async Task<IEnumerable<PostDTO>> GetFlaggedPostsAsync()
        {
            // Get all reports for posts (TargetType == "post")
            var postReports = await _reportRepository.GetReportsByTargetTypeAsync("post");
            
            // Get unique post IDs from reports
            var flaggedPostIds = postReports.Select(r => r.TargetId).Distinct().ToList();
            
            if (!flaggedPostIds.Any())
            {
                return Enumerable.Empty<PostDTO>();
            }

            // Get all flagged posts, excluding approved and deleted posts
            var flaggedPosts = new List<PostDTO>();
            foreach (var postId in flaggedPostIds)
            {
                var post = await _postRepository.GetPostByIdAsync(postId);
                // Only include posts that are not approved or deleted
                if (post != null && !string.IsNullOrWhiteSpace(post.Status) && post.Status.ToLowerInvariant() != "approved" && post.Status.ToLowerInvariant() != "deleted")
                {
                    var (reactionCount, dislikeCount) = await CalculateReactionsAndDislikes(post.Id);
                    var commentCount = await CalculateCommentCount(post.Id);
                    flaggedPosts.Add(new PostDTO(
                        post.Id,
                        post.UserId,
                        post.Title,
                        post.Content,
                        post.Visibility,
                        post.Status,
                        post.CreatedAt,
                        post.UpdatedAt,
                        reactionCount,
                        dislikeCount,
                        commentCount
                    ));
                }
            }

            return flaggedPosts;
        }
    }
}

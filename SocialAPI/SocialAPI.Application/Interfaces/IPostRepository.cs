using SocialAPI.Domain.Entities;

namespace SocialAPI.Application.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetPostsAsync();
        Task<Post> GetPostByIdAsync(Guid id);
        Task<Post> CreatePostAsync(Post post);
        Task UpdatePostAsync(Post post);
        Task DeletePostAsync(Guid id);

        Task<PostVote> GetPostVoteAsync(Guid postId, Guid userId);
        Task AddPostVoteAsync(PostVote vote);
        Task UpdatePostVoteAsync(PostVote vote);
        Task RemovePostVoteAsync(PostVote vote);
        Task<IEnumerable<PostVote>> GetVotesForPostAsync(Guid postId);

        Task<IEnumerable<Post>> GetPostsByTagAsync(string tagSlug);
    }
}

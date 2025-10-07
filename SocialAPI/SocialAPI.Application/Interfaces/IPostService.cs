using SocialAPI.Application.DTOs;

namespace SocialAPI.Application.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostDTO>> GetPostsAsync();
        Task<PostDTO> GetPostByIdAsync(Guid id);
        //Task UpdatePostAsync(Guid id, PostDTO postDto);
        Task<PostDTO> UpdatePostAsync(Guid id, PostDTO postDto);
        Task<PostDTO> UpdatePostStatusAsync(Guid id, string status);
        //Task UpdatePostStatusAsync(Guid id, string status);
        Task DeletePostAsync(Guid id);
        Task<PostDTO> CreatePostAsync(PostDTO postDto);

        Task VotePostAsync(Guid postId, Guid userId, string voteType);
        Task CancelPostVoteAsync(Guid postId, Guid userId);

        Task<IEnumerable<PostDTO>> GetPostsByTagAsync(string tagSlug);
    }
}

namespace SocialAPI.Application.DTOs
{
    public record CreateCommentDTO
    (
        string Content,
        Guid? ParentCommentId
    );
}

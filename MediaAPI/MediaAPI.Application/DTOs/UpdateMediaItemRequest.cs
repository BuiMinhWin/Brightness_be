
namespace MediaAPI.Application.DTOs
{
    public record UpdateMediaItemRequest
    (
        string? Title,
        string? Description,
        string? Url,
        string? Thumbnail,
        string? Category,
        string? Duration,
        string? Platform,
        string? Source,
        string? Status
    );
}

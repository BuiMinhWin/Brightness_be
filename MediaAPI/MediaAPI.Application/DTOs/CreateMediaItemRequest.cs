
namespace MediaAPI.Application.DTOs
{
    public record CreateMediaItemRequest
    (
        string Title,
        string? Description,
        string Url,
        string? Thumbnail,
        string? Category,
        string? Duration,
        string? Platform,
        string? Source
    );
}

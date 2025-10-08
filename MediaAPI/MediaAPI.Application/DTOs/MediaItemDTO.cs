using System;

namespace MediaAPI.Application.DTOs
{
    public record MediaItemDTO
    (
        Guid Id,
        string? Title,
        string? Description,
        string Url,
        string? Thumbnail,
        int Views,
        int Likes,
        string? Category,
        string? Duration,
        string? Platform,
        string? Source,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}

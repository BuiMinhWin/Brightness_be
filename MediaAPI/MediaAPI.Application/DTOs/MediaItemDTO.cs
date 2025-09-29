using System;

namespace MediaAPI.Application.DTOs
{
    public record MediaItemDTO
    (
        Guid Id,
        string? Title,
        string? Description,
        string Url,
        string? Platform,
        string? Source,
        string Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}

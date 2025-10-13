namespace MediaAPI.Application.DTOs
{
    public record PaginatedMediaResponse
    (
        IEnumerable<MediaItemDTO> Items,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );
}


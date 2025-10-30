using PersonalCoachAPI.Application.DTOs;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IBrokenLinkReportService
    {
        Task<BrokenLinkReportDTO> CreateReportAsync(CreateBrokenLinkReportRequest request);
        Task<List<BrokenLinkReportListItemDTO>> GetAllReportsAsync(string? status = null);
        Task<BrokenLinkReportDTO> GetReportByIdAsync(Guid id);
        Task<BrokenLinkReportDTO> UpdateReportAsync(Guid id, UpdateBrokenLinkReportRequest request);
        Task<BrokenLinkReportDTO> ResolveReportAsync(Guid id, ResolveBrokenLinkRequest request);
        Task DeleteReportAsync(Guid id);
    }
}


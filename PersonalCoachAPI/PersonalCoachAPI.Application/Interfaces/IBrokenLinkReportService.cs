using PersonalCoachAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IBrokenLinkReportService
    {
        Task<BrokenLinkReportResponse> ReportBrokenLinkAsync(ReportBrokenLinkRequest request);
        Task<List<BrokenLinkReportListItem>> GetAllReportsAsync(string? status = null);
        Task<BrokenLinkReportResponse> GetReportByIdAsync(Guid id);
        Task<BrokenLinkReportResponse> UpdateReportAsync(Guid id, UpdateBrokenLinkRequest request);
        Task<BrokenLinkReportResponse> ResolveBrokenLinkAsync(Guid id, ResolveBrokenLinkRequest request);
        Task DeleteReportAsync(Guid id);
    }
}


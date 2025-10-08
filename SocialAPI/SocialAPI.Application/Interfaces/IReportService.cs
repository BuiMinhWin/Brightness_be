using SocialAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialAPI.Application.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<ReportDTO>> GetReportsAsync();
        Task<ReportDTO> GetReportByIdAsync(Guid reportId);
        Task<ReportDTO> CreateReportAsync(CreateReportDTO reportDto, Guid userId);
        Task ReviewReportAsync(Guid reportId, ReviewReportDTO reviewDto, Guid adminId);
        Task DeleteReportAsync(Guid reportId);
    }
}

using SocialAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialAPI.Application.Interfaces
{
    public interface IReportRepository
    {
        Task<IEnumerable<Report>> GetReportsAsync();
        Task<Report> GetReportByIdAsync(Guid reportId);
        Task<Report> CreateReportAsync(Report report);
        Task UpdateReportAsync(Report report);
        Task DeleteReportAsync(Guid reportId);
        Task<int> GetReportCountForTargetAsync(Guid targetId, string targetType);
        Task<IEnumerable<Report>> GetReportsByTargetAsync(Guid targetId, string targetType);
        Task<Report?> GetReportByUserAndTargetAsync(Guid reporterId, Guid targetId, string targetType);
    }
}

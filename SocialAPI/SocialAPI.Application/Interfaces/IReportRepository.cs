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
    }
}

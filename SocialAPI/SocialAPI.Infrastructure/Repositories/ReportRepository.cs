using Microsoft.EntityFrameworkCore;
using SocialAPI.Application.Interfaces;
using SocialAPI.Domain.Entities;
using SocialAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialAPI.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly SocialDbContext _context;

        public ReportRepository(SocialDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Report>> GetReportsAsync()
        {
            return await _context.Reports.ToListAsync();
        }

        public async Task<Report> GetReportByIdAsync(Guid reportId)
        {
            return await _context.Reports.FindAsync(reportId);
        }

        public async Task<Report> CreateReportAsync(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task UpdateReportAsync(Report report)
        {
            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteReportAsync(Guid reportId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetReportCountForTargetAsync(Guid targetId, string targetType)
        {
            return await _context.Reports
                .Where(r => r.TargetId == targetId && r.TargetType == targetType && r.Status == "pending")
                .CountAsync();
        }

        public async Task<IEnumerable<Report>> GetReportsByTargetAsync(Guid targetId, string targetType)
        {
            return await _context.Reports
                .Where(r => r.TargetId == targetId && r.TargetType == targetType)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Report?> GetReportByUserAndTargetAsync(Guid reporterId, Guid targetId, string targetType)
        {
            return await _context.Reports
                .FirstOrDefaultAsync(r => r.ReporterId == reporterId && r.TargetId == targetId && r.TargetType == targetType && r.Status == "pending");
        }
    }
}

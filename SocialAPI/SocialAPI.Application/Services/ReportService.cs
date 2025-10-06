using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using SocialAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialAPI.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<IEnumerable<ReportDTO>> GetReportsAsync()
        {
            var reports = await _reportRepository.GetReportsAsync();
            return reports.Select(r => new ReportDTO(r.Id, r.ReporterId, r.TargetType, r.TargetId, r.Reason, r.Status, r.Notes, r.ReviewerId, r.CreatedAt, r.ReviewedAt));
        }

        public async Task<ReportDTO> GetReportByIdAsync(Guid reportId)
        {
            var report = await _reportRepository.GetReportByIdAsync(reportId);
            if (report == null) return null;
            return new ReportDTO(report.Id, report.ReporterId, report.TargetType, report.TargetId, report.Reason, report.Status, report.Notes, report.ReviewerId, report.CreatedAt, report.ReviewedAt);
        }

        public async Task<ReportDTO> CreateReportAsync(CreateReportDTO reportDto, Guid userId)
        {
            var report = new Report
            {
                Id = Guid.NewGuid(),
                ReporterId = userId,
                TargetId = reportDto.TargetId,
                TargetType = reportDto.TargetType,
                Reason = reportDto.Reason,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            var createdReport = await _reportRepository.CreateReportAsync(report);
            return new ReportDTO(createdReport.Id, createdReport.ReporterId, createdReport.TargetType, createdReport.TargetId, createdReport.Reason, createdReport.Status, createdReport.Notes, createdReport.ReviewerId, createdReport.CreatedAt, createdReport.ReviewedAt);
        }

        public async Task ReviewReportAsync(Guid reportId, ReviewReportDTO reviewDto, Guid adminId)
        {
            var report = await _reportRepository.GetReportByIdAsync(reportId);
            if (report == null) throw new Exception("Report not found");

            report.Status = reviewDto.Status;
            report.ReviewedAt = DateTime.UtcNow;
            // You might want to store the adminId who reviewed the report

            await _reportRepository.UpdateReportAsync(report);
        }

        public async Task DeleteReportAsync(Guid reportId)
        {
            await _reportRepository.DeleteReportAsync(reportId);
        }
    }
}

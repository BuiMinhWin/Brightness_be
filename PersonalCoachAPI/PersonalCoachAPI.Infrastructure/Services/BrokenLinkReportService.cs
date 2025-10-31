using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class BrokenLinkReportService : IBrokenLinkReportService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<BrokenLinkReportService> _logger;

        public BrokenLinkReportService(
            PersonalCoachDbContext context,
            ILogger<BrokenLinkReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BrokenLinkReportDTO> CreateReportAsync(CreateBrokenLinkReportRequest request)
        {
            var report = new BrokenLinkReport
            {
                Id = Guid.NewGuid(),
                ExerciseId = request.ExerciseId,
                ExerciseName = request.ExerciseName,
                BrokenVideoUrl = request.BrokenVideoUrl,
                ReportedByUserId = request.UserId,
                ReportedAt = DateTime.UtcNow,
                Status = "pending",
                UserComment = request.UserComment
            };

            _context.BrokenLinkReports.Add(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Broken link report created for exercise {ExerciseId} by user {UserId}", 
                request.ExerciseId, request.UserId);

            return MapToDTO(report);
        }

        public async Task<List<BrokenLinkReportListItemDTO>> GetAllReportsAsync(string? status = null)
        {
            var query = _context.BrokenLinkReports.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var reports = await query
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();

            // Group by exercise ID to get report counts
            var groupedReports = reports
                .GroupBy(r => r.ExerciseId)
                .Select(g => new BrokenLinkReportListItemDTO
                {
                    Id = g.First().Id,
                    ExerciseId = g.Key,
                    ExerciseName = g.First().ExerciseName,
                    BrokenVideoUrl = g.First().BrokenVideoUrl,
                    ReportedAt = g.First().ReportedAt,
                    Status = g.First().Status,
                    NewVideoUrl = g.First().NewVideoUrl,
                    ReportCount = g.Count(),
                    AdminNotes = g.First().AdminNotes,
                    ResolvedAt = g.First().ResolvedAt,
                    UserComment = g.First().UserComment,
                    IsResolved = g.First().Status == "resolved"
                })
                .ToList();

            return groupedReports;
        }

        public async Task<BrokenLinkReportDTO> GetReportByIdAsync(Guid id)
        {
            var report = await _context.BrokenLinkReports.FindAsync(id);
            if (report == null)
            {
                throw new ArgumentException($"Broken link report with ID {id} not found");
            }

            return MapToDTO(report);
        }

        public async Task<BrokenLinkReportDTO> UpdateReportAsync(Guid id, UpdateBrokenLinkReportRequest request)
        {
            var report = await _context.BrokenLinkReports.FindAsync(id);
            if (report == null)
            {
                throw new ArgumentException($"Broken link report with ID {id} not found");
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                report.Status = request.Status;
            }

            if (!string.IsNullOrEmpty(request.AdminNotes))
            {
                report.AdminNotes = request.AdminNotes;
            }

            if (!string.IsNullOrEmpty(request.NewVideoUrl))
            {
                report.NewVideoUrl = request.NewVideoUrl;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Broken link report {ReportId} updated", id);

            return MapToDTO(report);
        }

        public async Task<BrokenLinkReportDTO> ResolveReportAsync(Guid id, ResolveBrokenLinkRequest request)
        {
            var report = await _context.BrokenLinkReports.FindAsync(id);
            if (report == null)
            {
                throw new ArgumentException($"Broken link report with ID {id} not found");
            }

            report.Status = "resolved";
            report.NewVideoUrl = request.NewVideoUrl;
            report.AdminNotes = request.AdminNotes;
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolvedByUserId = request.AdminUserId;

            // Update the exercise in the library with the new video URL
            if (Guid.TryParse(report.ExerciseId, out var exerciseGuid))
            {
                var exercise = await _context.ExerciseLibrary
                    .FirstOrDefaultAsync(e => e.Id == exerciseGuid);

                if (exercise != null)
                {
                    exercise.VideoUrl = request.NewVideoUrl;
                    _logger.LogInformation("Updated exercise {ExerciseId} video URL to {NewUrl}", 
                        report.ExerciseId, request.NewVideoUrl);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Broken link report {ReportId} resolved by admin {AdminUserId}", 
                id, request.AdminUserId);

            return MapToDTO(report);
        }

        public async Task DeleteReportAsync(Guid id)
        {
            var report = await _context.BrokenLinkReports.FindAsync(id);
            if (report == null)
            {
                throw new ArgumentException($"Broken link report with ID {id} not found");
            }

            _context.BrokenLinkReports.Remove(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Broken link report {ReportId} deleted", id);
        }

        private BrokenLinkReportDTO MapToDTO(BrokenLinkReport report)
        {
            return new BrokenLinkReportDTO
            {
                Id = report.Id,
                ExerciseId = report.ExerciseId,
                ExerciseName = report.ExerciseName,
                BrokenVideoUrl = report.BrokenVideoUrl,
                ReportedByUserId = report.ReportedByUserId,
                ReportedAt = report.ReportedAt,
                Status = report.Status,
                UserComment = report.UserComment,
                AdminNotes = report.AdminNotes,
                ResolvedAt = report.ResolvedAt,
                ResolvedByUserId = report.ResolvedByUserId,
                NewVideoUrl = report.NewVideoUrl
            };
        }
    }
}


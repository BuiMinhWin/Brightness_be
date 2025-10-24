using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Infrastructure.Services
{
    public class BrokenLinkReportService : IBrokenLinkReportService
    {
        private readonly PersonalCoachDbContext _context;
        private readonly ILogger<BrokenLinkReportService> _logger;

        public BrokenLinkReportService(PersonalCoachDbContext context, ILogger<BrokenLinkReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BrokenLinkReportResponse> ReportBrokenLinkAsync(ReportBrokenLinkRequest request)
        {
            _logger.LogInformation("User {UserId} reporting broken link for exercise {ExerciseId}", 
                request.UserId, request.ExerciseId);

            var report = new BrokenLinkReport
            {
                Id = Guid.NewGuid(),
                ExerciseId = request.ExerciseId,
                ExerciseName = request.ExerciseName,
                BrokenVideoUrl = request.BrokenVideoUrl,
                ReportedByUserId = request.UserId,
                ReportedAt = DateTime.UtcNow,
                Status = "pending",
                AdminNotes = request.UserComment,
                CreatedAt = DateTime.UtcNow
            };

            _context.BrokenLinkReports.Add(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Broken link report {ReportId} created successfully", report.Id);

            return MapToResponse(report);
        }

        public async Task<List<BrokenLinkReportListItem>> GetAllReportsAsync(string? status = null)
        {
            var query = _context.BrokenLinkReports.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status.ToLower());
            }

            var reports = await query
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync();

            // Group by exercise and count reports
            var groupedReports = reports
                .GroupBy(r => new { r.ExerciseId, r.ExerciseName, r.BrokenVideoUrl, r.Status })
                .Select(g => new BrokenLinkReportListItem
                {
                    Id = g.First().Id,
                    ExerciseId = g.Key.ExerciseId,
                    ExerciseName = g.Key.ExerciseName,
                    BrokenVideoUrl = g.Key.BrokenVideoUrl,
                    ReportedAt = g.Max(r => r.ReportedAt),
                    Status = g.Key.Status,
                    NewVideoUrl = g.First().NewVideoUrl,
                    ReportCount = g.Count()
                })
                .ToList();

            return groupedReports;
        }

        public async Task<BrokenLinkReportResponse> GetReportByIdAsync(Guid id)
        {
            var report = await _context.BrokenLinkReports.FindAsync(id);
            
            if (report == null)
            {
                throw new ArgumentException($"Report {id} not found");
            }

            return MapToResponse(report);
        }

        public async Task<BrokenLinkReportResponse> UpdateReportAsync(Guid id, UpdateBrokenLinkRequest request)
        {
            var report = await _context.BrokenLinkReports.FindAsync(id);
            
            if (report == null)
            {
                throw new ArgumentException($"Report {id} not found");
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                report.Status = request.Status.ToLower();
            }

            if (request.AdminNotes != null)
            {
                report.AdminNotes = request.AdminNotes;
            }

            if (request.NewVideoUrl != null)
            {
                report.NewVideoUrl = request.NewVideoUrl;
            }

            report.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Report {ReportId} updated to status: {Status}", id, report.Status);

            return MapToResponse(report);
        }

        public async Task<BrokenLinkReportResponse> ResolveBrokenLinkAsync(Guid id, ResolveBrokenLinkRequest request)
        {
            var report = await _context.BrokenLinkReports.FindAsync(id);
            
            if (report == null)
            {
                throw new ArgumentException($"Report {id} not found");
            }

            // Update the exercise in the library with the new video URL
            // Since WorkoutScheduleExercise doesn't have ExerciseLibraryId, we search by name
            _logger.LogInformation("Looking for exercise by name: {ExerciseName} in library", report.ExerciseName);
            
            var exercise = await _context.ExerciseLibrary
                .FirstOrDefaultAsync(e => e.ExerciseName == report.ExerciseName || e.ExerciseNameVi == report.ExerciseName);

            if (exercise != null)
            {
                _logger.LogInformation("Found exercise: {ExerciseName} (ID: {ExerciseId}), updating video URL from {OldUrl} to {NewUrl}", 
                    exercise.ExerciseName, exercise.Id, exercise.VideoUrl, request.NewVideoUrl);
                    
                exercise.VideoUrl = request.NewVideoUrl;
                exercise.UpdatedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Exercise VideoUrl property updated in memory");
            }
            else
            {
                _logger.LogWarning("Exercise '{ExerciseName}' not found in library! Cannot update video URL. This might be a custom exercise.", report.ExerciseName);
            }

            // Mark report as resolved
            report.Status = "resolved";
            report.NewVideoUrl = request.NewVideoUrl;
            report.AdminNotes = request.AdminNotes;
            report.ResolvedAt = DateTime.UtcNow;
            report.ResolvedByUserId = request.AdminUserId;
            report.UpdatedAt = DateTime.UtcNow;

            // Also resolve all other pending reports for the same exercise (by name)
            var otherReports = await _context.BrokenLinkReports
                .Where(r => r.ExerciseName == report.ExerciseName && r.Status == "pending" && r.Id != id)
                .ToListAsync();

            foreach (var otherReport in otherReports)
            {
                otherReport.Status = "resolved";
                otherReport.NewVideoUrl = request.NewVideoUrl;
                otherReport.ResolvedAt = DateTime.UtcNow;
                otherReport.ResolvedByUserId = request.AdminUserId;
                otherReport.UpdatedAt = DateTime.UtcNow;
            }

            var changeCount = await _context.SaveChangesAsync();

            _logger.LogInformation("Database SaveChanges completed. {ChangeCount} entities updated", changeCount);
            _logger.LogInformation("Report {ReportId} resolved successfully", id);

            return MapToResponse(report);
        }

        public async Task DeleteReportAsync(Guid id)
        {
            var report = await _context.BrokenLinkReports.FindAsync(id);
            
            if (report == null)
            {
                throw new ArgumentException($"Report {id} not found");
            }

            _context.BrokenLinkReports.Remove(report);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Report {ReportId} deleted", id);
        }

        private BrokenLinkReportResponse MapToResponse(BrokenLinkReport report)
        {
            return new BrokenLinkReportResponse
            {
                Id = report.Id,
                ExerciseId = report.ExerciseId,
                ExerciseName = report.ExerciseName,
                BrokenVideoUrl = report.BrokenVideoUrl,
                ReportedByUserId = report.ReportedByUserId,
                ReportedAt = report.ReportedAt,
                Status = report.Status,
                AdminNotes = report.AdminNotes,
                ResolvedAt = report.ResolvedAt,
                ResolvedByUserId = report.ResolvedByUserId,
                NewVideoUrl = report.NewVideoUrl
            };
        }
    }
}


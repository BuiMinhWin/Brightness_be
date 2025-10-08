using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialAPI.Application.DTOs;
using SocialAPI.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReportDTO>>> GetReports()
        {
            var reports = await _reportService.GetReportsAsync();
            return Ok(reports);
        }

        //[Authorize]
        [HttpPost]
        public async Task<ActionResult<ReportDTO>> CreateReport(CreateReportDTO createReportDto)
        {
            //var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"); // dùng để test tạm thời
            var report = await _reportService.CreateReportAsync(createReportDto, userId);
            return CreatedAtAction(nameof(GetReport), new { reportId = report.Id }, report);
        }

        [HttpGet("{reportId}")]
        public async Task<ActionResult<ReportDTO>> GetReport(Guid reportId)
        {
            var report = await _reportService.GetReportByIdAsync(reportId);
            if (report == null)
            {
                return NotFound();
            }
            return Ok(report);
        }

        //[Authorize(Roles = "Admin")]
        [HttpPatch("{reportId}/review")]
        public async Task<IActionResult> ReviewReport(Guid reportId, ReviewReportDTO reviewDto)
        {
            //var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var adminId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"); // dùng để test tạm thời
            await _reportService.ReviewReportAsync(reportId, reviewDto, adminId);
            return Ok("Update Status Success");
        }

        //[Authorize(Roles = "Admin")]
        [HttpDelete("{reportId}")]
        public async Task<IActionResult> DeleteReport(Guid reportId)
        {
            await _reportService.DeleteReportAsync(reportId);
            return Ok("Delete Success");
        }
    }
}

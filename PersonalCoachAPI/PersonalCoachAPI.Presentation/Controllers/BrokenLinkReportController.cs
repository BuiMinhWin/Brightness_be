using Microsoft.AspNetCore.Mvc;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;

namespace PersonalCoachAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrokenLinkReportController : ControllerBase
    {
        private readonly IBrokenLinkReportService _reportService;
        private readonly ILogger<BrokenLinkReportController> _logger;

        public BrokenLinkReportController(
            IBrokenLinkReportService reportService,
            ILogger<BrokenLinkReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>
        /// Report a broken video link (User endpoint)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BrokenLinkReportDTO>> ReportBrokenLink(
            [FromBody] CreateBrokenLinkReportRequest request)
        {
            try
            {
                var result = await _reportService.CreateReportAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating broken link report");
                return StatusCode(500, new { success = false, message = "Failed to create report" });
            }
        }

        /// <summary>
        /// Get all broken link reports (Admin endpoint)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<BrokenLinkReportListItemDTO>>> GetAllReports(
            [FromQuery] string? status = null)
        {
            try
            {
                var result = await _reportService.GetAllReportsAsync(status);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting broken link reports");
                return StatusCode(500, new { success = false, message = "Failed to get reports" });
            }
        }

        /// <summary>
        /// Get specific broken link report (Admin endpoint)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BrokenLinkReportDTO>> GetReportById(Guid id)
        {
            try
            {
                var result = await _reportService.GetReportByIdAsync(id);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting broken link report {ReportId}", id);
                return StatusCode(500, new { success = false, message = "Failed to get report" });
            }
        }

        /// <summary>
        /// Update broken link report status/notes (Admin endpoint)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult<BrokenLinkReportDTO>> UpdateReport(
            Guid id,
            [FromBody] UpdateBrokenLinkReportRequest request)
        {
            try
            {
                var result = await _reportService.UpdateReportAsync(id, request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating broken link report {ReportId}", id);
                return StatusCode(500, new { success = false, message = "Failed to update report" });
            }
        }

        /// <summary>
        /// Resolve broken link with new URL and update exercise (Admin endpoint)
        /// </summary>
        [HttpPut("{id}/resolve")]
        public async Task<ActionResult<BrokenLinkReportDTO>> ResolveBrokenLink(
            Guid id,
            [FromBody] ResolveBrokenLinkRequest request)
        {
            try
            {
                var result = await _reportService.ResolveReportAsync(id, request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving broken link report {ReportId}", id);
                return StatusCode(500, new { success = false, message = "Failed to resolve report" });
            }
        }

        /// <summary>
        /// Delete broken link report (Admin endpoint)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReport(Guid id)
        {
            try
            {
                await _reportService.DeleteReportAsync(id);
                return Ok(new { success = true, message = "Report deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting broken link report {ReportId}", id);
                return StatusCode(500, new { success = false, message = "Failed to delete report" });
            }
        }
    }
}


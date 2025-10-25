using Microsoft.AspNetCore.Mvc;
using PersonalCoachAPI.Application.DTOs;
using PersonalCoachAPI.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrokenLinkReportController : ControllerBase
    {
        private readonly IBrokenLinkReportService _service;
        private readonly ILogger<BrokenLinkReportController> _logger;

        public BrokenLinkReportController(
            IBrokenLinkReportService service,
            ILogger<BrokenLinkReportController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Report a broken video link (user endpoint)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BrokenLinkReportResponse>> ReportBrokenLink(
            [FromBody] ReportBrokenLinkRequest request)
        {
            try
            {
                var result = await _service.ReportBrokenLinkAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting broken link");
                return StatusCode(500, new { success = false, message = "Failed to report broken link" });
            }
        }

        /// <summary>
        /// Get all broken link reports (admin endpoint)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAllReports([FromQuery] string? status = null)
        {
            try
            {
                var reports = await _service.GetAllReportsAsync(status);
                return Ok(new { success = true, data = reports });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting broken link reports");
                return StatusCode(500, new { success = false, message = "Failed to get reports" });
            }
        }

        /// <summary>
        /// Get specific broken link report (admin endpoint)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BrokenLinkReportResponse>> GetReportById(Guid id)
        {
            try
            {
                var report = await _service.GetReportByIdAsync(id);
                return Ok(new { success = true, data = report });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report {ReportId}", id);
                return StatusCode(500, new { success = false, message = "Failed to get report" });
            }
        }

        /// <summary>
        /// Update broken link report status/notes (admin endpoint)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult<BrokenLinkReportResponse>> UpdateReport(
            Guid id,
            [FromBody] UpdateBrokenLinkRequest request)
        {
            try
            {
                var result = await _service.UpdateReportAsync(id, request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", id);
                return StatusCode(500, new { success = false, message = "Failed to update report" });
            }
        }

        /// <summary>
        /// Resolve broken link with new URL and update exercise (admin endpoint)
        /// </summary>
        [HttpPut("{id}/resolve")]
        public async Task<ActionResult<BrokenLinkReportResponse>> ResolveBrokenLink(
            Guid id,
            [FromBody] ResolveBrokenLinkRequest request)
        {
            try
            {
                var result = await _service.ResolveBrokenLinkAsync(id, request);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving broken link {ReportId}", id);
                return StatusCode(500, new { success = false, message = "Failed to resolve broken link" });
            }
        }

        /// <summary>
        /// Delete broken link report (admin endpoint)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReport(Guid id)
        {
            try
            {
                await _service.DeleteReportAsync(id);
                return Ok(new { success = true, message = "Report deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting report {ReportId}", id);
                return StatusCode(500, new { success = false, message = "Failed to delete report" });
            }
        }
    }
}


using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using MeasurementAPI.Application.Interfaces;
using MeasurementAPI.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace MeasurementAPI.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/measurements")]
    public class MeasurementsController(IBodyMeasurementService bodyMeasurementService) : ControllerBase
    {
        private readonly IBodyMeasurementService _bodyMeasurementService = bodyMeasurementService;

        [HttpPost]
        public async Task<IActionResult> RecordAsync([FromBody] CreateBodyMeasurementRequest request)
        {
            var response = await _bodyMeasurementService.RecordAsync(request);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var measurement = await _bodyMeasurementService.GetByIdAsync(id);
            return measurement is null ? NotFound() : Ok(measurement);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistoryAsync([FromQuery] Guid userId, [FromQuery] Guid? deviceId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool includeRaw = false)
        {
            var filter = new MeasurementHistoryFilter
            {
                DeviceId = deviceId,
                From = from,
                To = to,
                Page = page,
                PageSize = pageSize,
                IncludeRawData = includeRaw
            };

            var history = await _bodyMeasurementService.GetHistoryAsync(userId, filter);
            return Ok(history);
        }
    }
}

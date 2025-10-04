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
    [Route("api/measurements/devices")]
    public class DevicesController(IMeasurementDeviceService deviceService) : ControllerBase
    {
        private readonly IMeasurementDeviceService _deviceService = deviceService;

        [HttpGet]
        public async Task<IActionResult> GetByUserAsync([FromQuery] Guid userId)
        {
            var devices = await _deviceService.GetByUserAsync(userId);
            return Ok(devices);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var device = await _deviceService.GetByIdAsync(id);
            return device is null ? NotFound() : Ok(device);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDeviceRequest request)
        {
            var response = await _deviceService.RegisterAsync(request);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateDeviceRequest request)
        {
            var response = await _deviceService.UpdateAsync(id, request);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPost("{id:guid}/sync")]
        public async Task<IActionResult> RecordSyncAsync(Guid id, [FromBody] RecordDeviceSyncRequest request)
        {
            request.DeviceId = id;
            var response = await _deviceService.RecordSyncAsync(request);
            return response.Flag ? Ok(response) : BadRequest(response);
        }
    }
}

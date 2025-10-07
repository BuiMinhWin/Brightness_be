using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeasurementAPI.Application.DTOs;
using MeasurementAPI.Application.Requests;
using BrightNess.Library.Responses;

namespace MeasurementAPI.Application.Interfaces
{
    public interface IMeasurementDeviceService
    {
        Task<IEnumerable<MeasurementDeviceDTO>> GetByUserAsync(Guid userId);
        Task<MeasurementDeviceDTO?> GetByIdAsync(Guid deviceId);
        Task<Response> RegisterAsync(RegisterDeviceRequest request);
        Task<Response> UpdateAsync(Guid deviceId, UpdateDeviceRequest request);
        Task<Response> RecordSyncAsync(RecordDeviceSyncRequest request);
    }
}

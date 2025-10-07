using System;
using System.Threading.Tasks;
using MeasurementAPI.Application.DTOs;
using MeasurementAPI.Application.Requests;
using MeasurementAPI.Application.Responses;
using BrightNess.Library.Responses;

namespace MeasurementAPI.Application.Interfaces
{
    public interface IBodyMeasurementService
    {
        Task<Response> RecordAsync(CreateBodyMeasurementRequest request);
        Task<PagedResult<BodyMeasurementDTO>> GetHistoryAsync(Guid userId, MeasurementHistoryFilter filter);
        Task<BodyMeasurementDTO?> GetByIdAsync(Guid measurementId);
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using MeasurementAPI.Application.DTOs;
using MeasurementAPI.Application.Interfaces;
using MeasurementAPI.Application.Requests;
using MeasurementAPI.Application.Responses;
using MeasurementAPI.Domain.Entities;
using BrightNess.Library.Responses;

namespace MeasurementAPI.Application.Services
{
    public class BodyMeasurementService(IBodyMeasurementRepository repository, IMeasurementDeviceRepository deviceRepository) : IBodyMeasurementService
    {
        private readonly IBodyMeasurementRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMeasurementDeviceRepository _deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));

        public async Task<Response> RecordAsync(CreateBodyMeasurementRequest request)
        {
            MeasurementDevice? device = null;
            if (request.DeviceId.HasValue)
            {
                device = await _deviceRepository.FindByIdAsync(request.DeviceId.Value);
                if (device is null)
                {
                    return new Response(false, "Device not found");
                }
                if (device.UserId != request.UserId)
                {
                    return new Response(false, "Device does not belong to the specified user");
                }
            }

            var measurement = new BodyMeasurement
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                DeviceId = request.DeviceId,
                Source = request.Source,
                HeightCm = request.HeightCm,
                WeightKg = request.WeightKg,
                BodyFatPercent = request.BodyFatPercent,
                SkeletalMuscleMassKg = request.SkeletalMuscleMassKg,
                TotalBodyWaterPercent = request.TotalBodyWaterPercent,
                BasalMetabolicRate = request.BasalMetabolicRate,
                WaistCm = request.WaistCm,
                ChestCm = request.ChestCm,
                HipCm = request.HipCm,
                ActivityLevel = request.ActivityLevel,
                WorkoutDaysPerWeek = request.WorkoutDaysPerWeek,
                Goal = request.Goal,
                HealthNote = request.HealthNote,
                RawData = request.RawData,
                RecordedAt = request.RecordedAt,
                CreatedAt = DateTime.UtcNow,
                SyncedAt = request.DeviceId.HasValue ? DateTime.UtcNow : null
            };

            var response = await _repository.CreateAsync(measurement);
            if (response.Flag && device is not null)
            {
                var now = DateTime.UtcNow;
                device.LastSyncedAt = now;
                device.LastSyncStartedAt = request.RecordedAt;
                device.LastSyncCompletedAt = now;
                device.LastSyncSuccess = true;
                device.LastSyncMessage = "Measurement recorded";
                device.LastSyncPayload = request.RawData;
                await _deviceRepository.UpdateAsync(device);
            }

            return response;
        }

        public async Task<PagedResult<BodyMeasurementDTO>> GetHistoryAsync(Guid userId, MeasurementHistoryFilter filter)
        {
            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);
            var skip = (page - 1) * pageSize;

            var measurements = await _repository.GetHistoryAsync(userId, filter.DeviceId, filter.From, filter.To, skip, pageSize);
            var total = await _repository.CountHistoryAsync(userId, filter.DeviceId, filter.From, filter.To);

            var dtos = measurements.Select(MapToDto);
            if (!filter.IncludeRawData)
            {
                dtos = dtos.Select(dto => dto with { RawData = null });
            }

            return new PagedResult<BodyMeasurementDTO>(dtos.ToList(), page, pageSize, total);
        }

        public async Task<BodyMeasurementDTO?> GetByIdAsync(Guid measurementId)
        {
            var measurement = await _repository.FindByIdAsync(measurementId);
            return measurement is null ? null : MapToDto(measurement);
        }

        private static BodyMeasurementDTO MapToDto(BodyMeasurement measurement) => new(
            measurement.Id,
            measurement.UserId,
            measurement.DeviceId,
            measurement.Source,
            measurement.HeightCm,
            measurement.WeightKg,
            measurement.BodyFatPercent,
            measurement.SkeletalMuscleMassKg,
            measurement.TotalBodyWaterPercent,
            measurement.BasalMetabolicRate,
            measurement.WaistCm,
            measurement.ChestCm,
            measurement.HipCm,
            measurement.ActivityLevel,
            measurement.WorkoutDaysPerWeek,
            measurement.Goal,
            measurement.HealthNote,
            measurement.RawData,
            measurement.RecordedAt,
            measurement.CreatedAt,
            measurement.SyncedAt
        );
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeasurementAPI.Application.DTOs;
using MeasurementAPI.Application.Interfaces;
using MeasurementAPI.Application.Requests;
using MeasurementAPI.Domain.Entities;
using BrightNess.Library.Responses;

namespace MeasurementAPI.Application.Services
{
    public class MeasurementDeviceService(
        IMeasurementDeviceRepository deviceRepository,
        IDeviceSyncLogRepository syncLogRepository)
        : IMeasurementDeviceService
    {
        private readonly IMeasurementDeviceRepository _deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
        private readonly IDeviceSyncLogRepository _syncLogRepository = syncLogRepository ?? throw new ArgumentNullException(nameof(syncLogRepository));

        public async Task<IEnumerable<MeasurementDeviceDTO>> GetByUserAsync(Guid userId)
        {
            var devices = await _deviceRepository.GetByUserAsync(userId);
            return devices.Select(MapToDto);
        }

        public async Task<MeasurementDeviceDTO?> GetByIdAsync(Guid deviceId)
        {
            var device = await _deviceRepository.FindByIdAsync(deviceId);
            return device is null ? null : MapToDto(device);
        }

        public async Task<Response> RegisterAsync(RegisterDeviceRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                var existing = await _deviceRepository.GetBySerialAsync(request.UserId, request.SerialNumber);
                if (existing is not null)
                {
                    return new Response(false, "Device with this serial number already registered for user");
                }
            }

            var device = new MeasurementDevice
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                DeviceType = request.DeviceType.Trim(),
                Manufacturer = request.Manufacturer?.Trim() ?? string.Empty,
                Model = request.Model?.Trim() ?? string.Empty,
                SerialNumber = string.IsNullOrWhiteSpace(request.SerialNumber) ? null : request.SerialNumber.Trim(),
                FirmwareVersion = string.IsNullOrWhiteSpace(request.FirmwareVersion) ? null : request.FirmwareVersion.Trim(),
                RegisteredAt = DateTime.UtcNow,
                IsActive = true
            };

            return await _deviceRepository.CreateAsync(device);
        }

        public async Task<Response> UpdateAsync(Guid deviceId, UpdateDeviceRequest request)
        {
            var device = await _deviceRepository.FindByIdAsync(deviceId);
            if (device is null)
            {
                return new Response(false, "Device not found");
            }

            if (!string.IsNullOrWhiteSpace(request.Manufacturer))
            {
                device.Manufacturer = request.Manufacturer.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Model))
            {
                device.Model = request.Model.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                var existing = await _deviceRepository.GetBySerialAsync(device.UserId, request.SerialNumber.Trim());
                if (existing is not null && existing.Id != device.Id)
                {
                    return new Response(false, "Another device with this serial number already exists");
                }
                device.SerialNumber = request.SerialNumber.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.FirmwareVersion))
            {
                device.FirmwareVersion = request.FirmwareVersion.Trim();
            }

            if (request.IsActive.HasValue)
            {
                device.IsActive = request.IsActive.Value;
            }

            return await _deviceRepository.UpdateAsync(device);
        }

        public async Task<Response> RecordSyncAsync(RecordDeviceSyncRequest request)
        {
            var device = await _deviceRepository.FindByIdAsync(request.DeviceId);
            if (device is null)
            {
                return new Response(false, "Device not found");
            }

            var log = new DeviceSyncLog
            {
                Id = Guid.NewGuid(),
                DeviceId = device.Id,
                SyncStartedAt = request.SyncStartedAt,
                SyncCompletedAt = request.SyncCompletedAt,
                Success = request.Success,
                Message = request.Message,
                RawPayload = request.RawPayload
            };

            var response = await _syncLogRepository.CreateAsync(log);
            if (response.Flag)
            {
                device.LastSyncedAt = request.SyncCompletedAt ?? DateTime.UtcNow;
                await _deviceRepository.UpdateAsync(device);
            }

            return response;
        }

        public async Task<IEnumerable<DeviceSyncLogDTO>> GetSyncLogsAsync(Guid deviceId, int take = 20)
        {
            var logs = await _syncLogRepository.GetRecentAsync(deviceId, take);
            return logs.Select(log => new DeviceSyncLogDTO(
                log.Id,
                log.DeviceId,
                log.SyncStartedAt,
                log.SyncCompletedAt,
                log.Success,
                log.Message
            ));
        }

        private static MeasurementDeviceDTO MapToDto(MeasurementDevice device) => new(
            device.Id,
            device.UserId,
            device.DeviceType,
            device.Manufacturer,
            device.Model,
            device.SerialNumber,
            device.FirmwareVersion,
            device.IsActive,
            device.RegisteredAt,
            device.LastSyncedAt
        );
    }
}

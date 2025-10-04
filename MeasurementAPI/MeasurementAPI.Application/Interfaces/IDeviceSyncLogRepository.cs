using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrightNess.Library.Interface;
using MeasurementAPI.Domain.Entities;

namespace MeasurementAPI.Application.Interfaces
{
    public interface IDeviceSyncLogRepository : IGenericInterface<DeviceSyncLog>
    {
        Task<IEnumerable<DeviceSyncLog>> GetRecentAsync(Guid deviceId, int take = 20);
    }
}

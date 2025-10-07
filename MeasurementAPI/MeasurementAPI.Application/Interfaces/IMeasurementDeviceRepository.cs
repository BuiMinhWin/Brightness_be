using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrightNess.Library.Interface;
using MeasurementAPI.Domain.Entities;

namespace MeasurementAPI.Application.Interfaces
{
    public interface IMeasurementDeviceRepository : IGenericInterface<MeasurementDevice>
    {
        Task<IEnumerable<MeasurementDevice>> GetByUserAsync(Guid userId);
        Task<MeasurementDevice?> GetBySerialAsync(Guid userId, string serialNumber);
    }
}

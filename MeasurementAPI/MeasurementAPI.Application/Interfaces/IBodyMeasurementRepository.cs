using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrightNess.Library.Interface;
using MeasurementAPI.Domain.Entities;

namespace MeasurementAPI.Application.Interfaces
{
    public interface IBodyMeasurementRepository : IGenericInterface<BodyMeasurement>
    {
        Task<IEnumerable<BodyMeasurement>> GetHistoryAsync(Guid userId, Guid? deviceId, DateTime? from, DateTime? to, int skip, int take);
        Task<int> CountHistoryAsync(Guid userId, Guid? deviceId, DateTime? from, DateTime? to);
    }
}

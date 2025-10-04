using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeasurementAPI.Application.Interfaces;
using MeasurementAPI.Domain.Entities;
using MeasurementAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MeasurementAPI.Infrastructure.Repositories
{
    public class BodyMeasurementRepository : GenericRepository<BodyMeasurement>, IBodyMeasurementRepository
    {
        public BodyMeasurementRepository(MeasurementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BodyMeasurement>> GetHistoryAsync(Guid userId, Guid? deviceId, DateTime? from, DateTime? to, int skip, int take)
        {
            var query = Context.BodyMeasurements
                .Include(measurement => measurement.Device)
                .AsNoTracking()
                .Where(measurement => measurement.UserId == userId);

            if (deviceId.HasValue)
            {
                query = query.Where(measurement => measurement.DeviceId == deviceId);
            }

            if (from.HasValue)
            {
                query = query.Where(measurement => measurement.RecordedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(measurement => measurement.RecordedAt <= to.Value);
            }

            return await query
                .OrderByDescending(measurement => measurement.RecordedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> CountHistoryAsync(Guid userId, Guid? deviceId, DateTime? from, DateTime? to)
        {
            var query = Context.BodyMeasurements
                .AsNoTracking()
                .Where(measurement => measurement.UserId == userId);

            if (deviceId.HasValue)
            {
                query = query.Where(measurement => measurement.DeviceId == deviceId);
            }

            if (from.HasValue)
            {
                query = query.Where(measurement => measurement.RecordedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(measurement => measurement.RecordedAt <= to.Value);
            }

            return await query.CountAsync();
        }
    }
}

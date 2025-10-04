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
    public class DeviceSyncLogRepository : GenericRepository<DeviceSyncLog>, IDeviceSyncLogRepository
    {
        public DeviceSyncLogRepository(MeasurementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DeviceSyncLog>> GetRecentAsync(Guid deviceId, int take = 20)
        {
            return await Context.DeviceSyncLogs
                .AsNoTracking()
                .Where(log => log.DeviceId == deviceId)
                .OrderByDescending(log => log.SyncStartedAt)
                .Take(take)
                .ToListAsync();
        }
    }
}

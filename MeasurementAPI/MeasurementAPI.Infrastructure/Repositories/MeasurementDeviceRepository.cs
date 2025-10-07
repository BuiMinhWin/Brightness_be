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
    public class MeasurementDeviceRepository : GenericRepository<MeasurementDevice>, IMeasurementDeviceRepository
    {
        public MeasurementDeviceRepository(MeasurementDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MeasurementDevice>> GetByUserAsync(Guid userId)
        {
            return await Context.MeasurementDevices
                .AsNoTracking()
                .Where(device => device.UserId == userId)
                .OrderBy(device => device.DeviceType)
                .ThenBy(device => device.RegisteredAt)
                .ToListAsync();
        }

        public async Task<MeasurementDevice?> GetBySerialAsync(Guid userId, string serialNumber)
        {
            return await Context.MeasurementDevices
                .AsNoTracking()
                .FirstOrDefaultAsync(device => device.UserId == userId && device.SerialNumber != null && device.SerialNumber == serialNumber);
        }

        public override async Task<MeasurementDevice?> FindByIdAsync(Guid id)
        {
            return await Context.MeasurementDevices.FirstOrDefaultAsync(device => device.Id == id);
        }
    }
}

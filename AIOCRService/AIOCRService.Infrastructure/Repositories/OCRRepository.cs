using AIOCRService.Application.Interfaces;
using AIOCRService.Domain.Entities;
using AIOCRService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIOCRService.Infrastructure.Repositories
{
    public class OCRRepository : IOCRRepository
    {
        private readonly AIOCRDbContext _context;

        public OCRRepository(AIOCRDbContext context)
        {
            _context = context;
        }

        public async Task<OCRRequest> CreateAsync(OCRRequest request)
        {
            _context.OCRRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<OCRRequest?> GetRequestByIdAsync(Guid id)
        {
            return await _context.OCRRequests
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<OCRResult?> GetResultByIdAsync(Guid id)
        {
            return await _context.OCRResults
                .Include(x => x.OCRRequest)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ProcessedData?> GetProcessedDataByIdAsync(Guid id)
        {
            return await _context.ProcessedData
                .Include(x => x.OCRResult)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<OCRResult> CreateResultAsync(OCRResult result)
        {
            _context.OCRResults.Add(result);
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<ProcessedData> CreateProcessedDataAsync(ProcessedData data)
        {
            _context.ProcessedData.Add(data);
            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<IEnumerable<OCRRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _context.OCRRequests
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProcessedData>> GetProcessedDataByUserIdAsync(Guid userId, bool latestOnly = true)
        {
            var query = _context.ProcessedData.Where(x => x.UserId == userId);
            
            if (latestOnly)
            {
                query = query.Where(x => x.IsLatest);
            }
            
            return await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProcessedData>> GetProcessedDataHistoryAsync(Guid userId, string dataType)
        {
            return await _context.ProcessedData
                .Where(x => x.UserId == userId && x.DataType == dataType)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkPreviousVersionsAsOldAsync(Guid? ocrResultId, Guid userId, string dataType)
        {
            IQueryable<ProcessedData> query = _context.ProcessedData
                .Where(x => x.UserId == userId && x.DataType == dataType && x.IsLatest);

            // If OCRResultId is provided, only mark versions for that specific OCR request
            if (ocrResultId.HasValue)
            {
                query = query.Where(x => x.OCRResultId == ocrResultId.Value);
            }

            var oldVersions = await query.ToListAsync();
            
            foreach (var old in oldVersions)
            {
                old.IsLatest = false;
            }

            if (oldVersions.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(OCRRequest request)
        {
            _context.OCRRequests.Update(request);
            await _context.SaveChangesAsync();
        }
    }
}


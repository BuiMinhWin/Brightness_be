using System;
using System.Threading.Tasks;

namespace AIOCRService.Application.Interfaces
{
    public interface IPersonalCoachApiService
    {
        Task<bool> CanAffordOCRProcessingAsync(Guid userId);
        Task DeductOCRProcessingCreditAsync(Guid userId);
    }
}


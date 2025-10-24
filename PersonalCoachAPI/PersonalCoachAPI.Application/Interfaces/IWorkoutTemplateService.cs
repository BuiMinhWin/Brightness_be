using PersonalCoachAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IWorkoutTemplateService
    {
        Task<List<WorkoutTemplateDTO>> GetAllTemplatesAsync(TemplateFilters? filters = null);
        Task<WorkoutTemplateDTO?> GetTemplateByIdAsync(Guid id);
    }
}





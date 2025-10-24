using PersonalCoachAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IExerciseLibraryService
    {
        Task<List<ExerciseLibraryDTO>> GetAllExercisesAsync(ExerciseFilters? filters = null);
        Task<ExerciseLibraryDTO?> GetExerciseByIdAsync(Guid id);
        Task<List<ExerciseLibraryDTO>> SearchExercisesAsync(string query);
        Task<List<string>> GetCategoriesAsync();
        Task<List<ExerciseLibraryDTO>> GetAISuggestionsAsync(ExerciseSuggestionRequest request);
    }
}





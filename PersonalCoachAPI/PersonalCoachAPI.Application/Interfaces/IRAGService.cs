using PersonalCoachAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Application.Interfaces
{
    public interface IRAGService
    {
        // Exercise search
        Task<List<ExerciseSearchResult>> SearchExercisesAsync(string query, int topK = 5);
        Task<List<ExerciseSearchResult>> SearchExercisesByGoalAsync(string goal, string fitnessLevel, int topK = 10);
        Task<List<ExerciseSearchResult>> SearchExercisesByGoalAsync(string goal, string fitnessLevel, List<string> availableEquipment, int topK = 10);
        
        // AI fallback for exercise generation
        Task<List<ExerciseSearchResult>> GenerateExercisesWithAIAsync(string focusArea, string difficultyLevel, List<string> availableEquipment, int count = 6);
        
        // Health knowledge search
        Task<List<HealthKnowledgeResult>> SearchHealthKnowledgeAsync(string query, int topK = 3);
        
        // User context retrieval
        Task<UserContextResult> GetUserContextAsync(Guid userId);
        Task UpdateUserContextAsync(Guid userId);
        
        // Generate embeddings
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}


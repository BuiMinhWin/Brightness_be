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
        
        // Health knowledge search
        Task<List<HealthKnowledgeResult>> SearchHealthKnowledgeAsync(string query, int topK = 3);
        
        // User context retrieval
        Task<UserContextResult> GetUserContextAsync(Guid userId);
        Task UpdateUserContextAsync(Guid userId);
        
        // Generate embeddings
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}


using System.Text.Json;
using Microsoft.Extensions.Logging;
using PersonalCoachAPI.Domain.Entities;
using PersonalCoachAPI.Infrastructure.Data;

namespace PersonalCoachAPI.Infrastructure.Services;

public class DatabaseSeederService
{
    private readonly PersonalCoachDbContext _dbContext;
    private readonly ILogger<DatabaseSeederService> _logger;

    public DatabaseSeederService(
        PersonalCoachDbContext dbContext,
        ILogger<DatabaseSeederService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedExerciseLibraryAsync();
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding database");
            throw;
        }
    }

    private async Task SeedExerciseLibraryAsync()
    {
        // Check if already seeded
        if (_dbContext.ExerciseLibrary.Any())
        {
            _logger.LogInformation("Exercise library already seeded, skipping...");
            return;
        }

        _logger.LogInformation("Seeding exercise library...");

        var seedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData", "exercise_library_sample.json");
        
        if (!File.Exists(seedFilePath))
        {
            _logger.LogWarning("Seed file not found at: {Path}", seedFilePath);
            return;
        }

        var json = await File.ReadAllTextAsync(seedFilePath);
        var exercises = JsonSerializer.Deserialize<List<ExerciseSeedDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (exercises == null || !exercises.Any())
        {
            _logger.LogWarning("No exercises found in seed file");
            return;
        }

        foreach (var dto in exercises)
        {
            var exercise = new ExerciseLibrary
            {
                Id = Guid.NewGuid(),
                ExerciseName = dto.ExerciseName,
                ExerciseNameVi = dto.ExerciseNameVi,
                Category = dto.Category,
                DifficultyLevel = dto.DifficultyLevel,
                EquipmentRequired = JsonSerializer.Serialize(dto.EquipmentRequired),
                TargetMuscles = JsonSerializer.Serialize(dto.TargetMuscles),
                SecondaryMuscles = JsonSerializer.Serialize(dto.SecondaryMuscles),
                Description = dto.Description,
                DescriptionVi = dto.DescriptionVi,
                Instructions = JsonSerializer.Serialize(dto.Instructions),
                Benefits = JsonSerializer.Serialize(dto.Benefits),
                CommonMistakes = JsonSerializer.Serialize(dto.CommonMistakes),
                Variations = JsonSerializer.Serialize(dto.Variations),
                VideoUrl = dto.VideoUrl ?? "",
                ImageUrl = dto.ImageUrl ?? "",
                CaloriesPerMinute = dto.CaloriesPerMinute,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmbeddingVector = null // Will be generated later by embedding service
            };

            _dbContext.ExerciseLibrary.Add(exercise);
        }

        _logger.LogInformation("Added {Count} exercises to database", exercises.Count);
    }

    private class ExerciseSeedDto
    {
        public string ExerciseName { get; set; } = "";
        public string ExerciseNameVi { get; set; } = "";
        public string Category { get; set; } = "";
        public string DifficultyLevel { get; set; } = "";
        public List<string> EquipmentRequired { get; set; } = new();
        public List<string> TargetMuscles { get; set; } = new();
        public List<string> SecondaryMuscles { get; set; } = new();
        public string Description { get; set; } = "";
        public string DescriptionVi { get; set; } = "";
        public List<string> Instructions { get; set; } = new();
        public List<string> Benefits { get; set; } = new();
        public List<string> CommonMistakes { get; set; } = new();
        public List<string> Variations { get; set; } = new();
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public decimal CaloriesPerMinute { get; set; }
    }
}


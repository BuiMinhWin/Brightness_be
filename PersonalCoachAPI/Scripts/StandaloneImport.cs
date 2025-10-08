using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Scripts
{
    class StandaloneImport
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  Personal Coach API - Import Exercises");
            Console.WriteLine("========================================");
            Console.WriteLine();

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<StandaloneImport>>();

                try
                {
                    logger.LogInformation("Starting exercise import process...");

                    // Get database context
                    var context = services.GetRequiredService<ImportDbContext>();
                    
                    logger.LogInformation("Ensuring database and tables exist...");
                    
                    // Create database if not exists
                    await context.Database.EnsureCreatedAsync();
                    
                    // Create table manually if not exists (without vector column for now)
                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS exercise_library (
                            ""Id"" UUID PRIMARY KEY,
                            ""ExerciseName"" VARCHAR(200) NOT NULL,
                            ""ExerciseNameVi"" VARCHAR(200),
                            ""Category"" VARCHAR(100),
                            ""DifficultyLevel"" VARCHAR(50),
                            ""EquipmentRequired"" JSONB,
                            ""TargetMuscles"" JSONB,
                            ""SecondaryMuscles"" JSONB,
                            ""Description"" TEXT,
                            ""DescriptionVi"" TEXT,
                            ""Instructions"" JSONB,
                            ""Benefits"" JSONB,
                            ""CommonMistakes"" JSONB,
                            ""Variations"" JSONB,
                            ""VideoUrl"" VARCHAR(500),
                            ""ImageUrl"" VARCHAR(500),
                            ""CaloriesPerMinute"" DECIMAL(5,2),
                            ""CreatedAt"" TIMESTAMP NOT NULL,
                            ""UpdatedAt"" TIMESTAMP NOT NULL
                        )");
                    
                    logger.LogInformation("Database and tables ready");

                    // Get JSON file path
                    var jsonFilePath = args.Length > 0 ? args[0] : "../SeedData/exercise_library_sample.json";
                    
                    if (!File.Exists(jsonFilePath))
                    {
                        logger.LogError("JSON file not found: {FilePath}", jsonFilePath);
                        Console.WriteLine($"ERROR: File not found - {jsonFilePath}");
                        return;
                    }

                    Console.WriteLine($"JSON file: {jsonFilePath}");
                    Console.WriteLine();

                    // Read and parse JSON
                    var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                    var exercises = JsonConvert.DeserializeObject<List<ExerciseModel>>(jsonContent);

                    if (exercises == null || !exercises.Any())
                    {
                        logger.LogWarning("No exercises found in JSON");
                        return;
                    }

                    logger.LogInformation("Found {Count} exercises to import", exercises.Count);
                    Console.WriteLine($"Found {exercises.Count} exercises to import");
                    Console.WriteLine();

                    // Clear existing data if table exists
                    try
                    {
                        var existingCount = await context.ExerciseLibrary.CountAsync();
                        if (existingCount > 0)
                        {
                            logger.LogInformation("Clearing {Count} existing exercises", existingCount);
                            context.ExerciseLibrary.RemoveRange(await context.ExerciseLibrary.ToListAsync());
                            await context.SaveChangesAsync();
                            Console.WriteLine($"Cleared {existingCount} existing exercises");
                        }
                    }
                    catch
                    {
                        // Table doesn't exist yet - that's OK
                        logger.LogInformation("Table doesn't exist yet, will be created");
                    }

                    // Import in batches
                    const int batchSize = 100;
                    var totalBatches = (int)Math.Ceiling((double)exercises.Count / batchSize);

                    for (int i = 0; i < totalBatches; i++)
                    {
                        var batch = exercises.Skip(i * batchSize).Take(batchSize).ToList();
                        await ImportBatch(context, batch, i + 1, totalBatches, logger);
                    }

                    Console.WriteLine();
                    Console.WriteLine("========================================");
                    Console.WriteLine($"  Successfully imported {exercises.Count} exercises!");
                    Console.WriteLine("========================================");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during import");
                    Console.WriteLine($"ERROR: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            }
        }

        static async Task ImportBatch(ImportDbContext context, List<ExerciseModel> batch, int batchNumber, int totalBatches, ILogger logger)
        {
            logger.LogInformation("Processing batch {BatchNumber}/{TotalBatches}", batchNumber, totalBatches);
            Console.WriteLine($"Processing batch {batchNumber}/{totalBatches}...");

            var entities = new List<ExerciseEntity>();

            foreach (var exercise in batch)
            {
                try
                {
                    var entity = new ExerciseEntity
                    {
                        Id = Guid.NewGuid(),
                        ExerciseName = exercise.ExerciseName,
                        ExerciseNameVi = exercise.ExerciseNameVi,
                        Category = exercise.Category,
                        DifficultyLevel = exercise.DifficultyLevel,
                        EquipmentRequired = JsonConvert.SerializeObject(exercise.EquipmentRequired),
                        TargetMuscles = JsonConvert.SerializeObject(exercise.TargetMuscles),
                        SecondaryMuscles = JsonConvert.SerializeObject(exercise.SecondaryMuscles),
                        Description = exercise.Description,
                        DescriptionVi = exercise.DescriptionVi,
                        Instructions = JsonConvert.SerializeObject(exercise.Instructions),
                        Benefits = JsonConvert.SerializeObject(exercise.Benefits),
                        CommonMistakes = JsonConvert.SerializeObject(exercise.CommonMistakes),
                        Variations = JsonConvert.SerializeObject(exercise.Variations),
                        VideoUrl = exercise.VideoUrl,
                        ImageUrl = "",
                        CaloriesPerMinute = (decimal)exercise.CaloriesPerMinute,
                        // EmbeddingVector = null, // Will be generated later
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    entities.Add(entity);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing exercise: {ExerciseName}", exercise.ExerciseName);
                }
            }

            // Bulk insert
            context.ExerciseLibrary.AddRange(entities);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully processed batch {BatchNumber}/{TotalBatches}", batchNumber, totalBatches);
            Console.WriteLine($"✓ Batch {batchNumber}/{totalBatches} completed ({entities.Count} exercises)");
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", optional: false)
                          .AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    Console.WriteLine($"Database: {connectionString?.Split(';')[2]}");

                    services.AddDbContext<ImportDbContext>(options =>
                    {
                        options.UseNpgsql(connectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.UseVector();
                        });
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                    });
                });
    }

    // Simple DbContext for import
    public class ImportDbContext : DbContext
    {
        public ImportDbContext(DbContextOptions<ImportDbContext> options) : base(options) { }
        
        public DbSet<ExerciseEntity> ExerciseLibrary => Set<ExerciseEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Don't try to add vector extension - will be added manually later
            
            modelBuilder.Entity<ExerciseEntity>(entity =>
            {
                entity.ToTable("exercise_library");
                entity.HasKey(x => x.Id);
            });
        }
    }

    // Entity model
    [Table("exercise_library")]
    public class ExerciseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string ExerciseName { get; set; } = "";
        public string ExerciseNameVi { get; set; } = "";
        public string Category { get; set; } = "";
        public string DifficultyLevel { get; set; } = "";
        [Column(TypeName = "jsonb")]
        public string EquipmentRequired { get; set; } = "";
        [Column(TypeName = "jsonb")]
        public string TargetMuscles { get; set; } = "";
        [Column(TypeName = "jsonb")]
        public string SecondaryMuscles { get; set; } = "";
        [Column(TypeName = "text")]
        public string Description { get; set; } = "";
        [Column(TypeName = "text")]
        public string DescriptionVi { get; set; } = "";
        [Column(TypeName = "jsonb")]
        public string Instructions { get; set; } = "";
        [Column(TypeName = "jsonb")]
        public string Benefits { get; set; } = "";
        [Column(TypeName = "jsonb")]
        public string CommonMistakes { get; set; } = "";
        [Column(TypeName = "jsonb")]
        public string Variations { get; set; } = "";
        public string VideoUrl { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal CaloriesPerMinute { get; set; }
        // Commented out for now - will add after installing pgvector extension
        // [Column(TypeName = "vector(1536)")]
        // public Vector? EmbeddingVector { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // JSON import model
    public class ExerciseModel
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
        public string VideoUrl { get; set; } = "";
        public double CaloriesPerMinute { get; set; }
    }
}

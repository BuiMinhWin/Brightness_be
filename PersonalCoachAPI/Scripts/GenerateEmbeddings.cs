using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pgvector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Scripts
{
    class GenerateEmbeddings
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("  Generate Embeddings for Exercises");
            Console.WriteLine("========================================");
            Console.WriteLine();

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<GenerateEmbeddings>>();
                var httpClient = services.GetRequiredService<HttpClient>();
                var configuration = services.GetRequiredService<IConfiguration>();

                try
                {
                    var apiKey = configuration["GoogleAI:ApiKey"];
                    var model = configuration["GoogleAI:Model"];

                    if (string.IsNullOrEmpty(apiKey))
                    {
                        Console.WriteLine("ERROR: Google AI API key not found in configuration");
                        return;
                    }

                    var context = services.GetRequiredService<EmbeddingDbContext>();
                    
                    // Get all exercises without embeddings
                    var exercises = await context.Exercises
                        .Where(x => x.EmbeddingVector == null)
                        .ToListAsync();

                    if (!exercises.Any())
                    {
                        Console.WriteLine("No exercises need embeddings. All done!");
                        return;
                    }

                    Console.WriteLine($"Found {exercises.Count} exercises needing embeddings");
                    Console.WriteLine();

                    int processed = 0;
                    int errors = 0;

                    foreach (var exercise in exercises)
                    {
                        try
                        {
                            // Generate embedding text
                            var embeddingText = $"{exercise.ExerciseName} {exercise.ExerciseNameVi} " +
                                              $"{exercise.Category} {exercise.DifficultyLevel} " +
                                              $"{exercise.Description} {exercise.DescriptionVi} " +
                                              $"{exercise.EquipmentRequired} {exercise.TargetMuscles}";

                            // Call Google AI to generate embedding
                            var embedding = await GenerateEmbeddingWithGoogleAI(httpClient, apiKey, embeddingText);

                            if (embedding != null && embedding.Length > 0)
                            {
                                exercise.EmbeddingVector = new Vector(embedding);
                                processed++;
                                
                                if (processed % 10 == 0)
                                {
                                    Console.WriteLine($"Processed {processed}/{exercises.Count} exercises...");
                                }
                            }
                            else
                            {
                                errors++;
                                logger.LogWarning("Failed to generate embedding for {ExerciseName}", exercise.ExerciseName);
                            }

                            // Save every 20 exercises to avoid losing progress
                            if (processed % 20 == 0)
                            {
                                await context.SaveChangesAsync();
                                Console.WriteLine($"✓ Saved progress ({processed} exercises)");
                            }

                            // Rate limiting - wait 1 second between calls
                            await Task.Delay(1000);
                        }
                        catch (Exception ex)
                        {
                            errors++;
                            logger.LogError(ex, "Error processing exercise: {ExerciseName}", exercise.ExerciseName);
                        }
                    }

                    // Final save
                    await context.SaveChangesAsync();

                    Console.WriteLine();
                    Console.WriteLine("========================================");
                    Console.WriteLine($"  Embeddings Generated!");
                    Console.WriteLine($"  Processed: {processed}");
                    Console.WriteLine($"  Errors: {errors}");
                    Console.WriteLine("========================================");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Fatal error during embedding generation");
                    Console.WriteLine($"ERROR: {ex.Message}");
                }
            }
        }

        static async Task<float[]?> GenerateEmbeddingWithGoogleAI(HttpClient httpClient, string apiKey, string text)
        {
            try
            {
                var url = "https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent?key=" + apiKey;

                var requestBody = new
                {
                    model = "models/text-embedding-004",
                    content = new
                    {
                        parts = new[] { new { text = text } }
                    }
                };

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Error: {response.StatusCode} - {responseContent}");
                    return null;
                }

                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var values = result?.embedding?.values;

                if (values == null)
                {
                    return null;
                }

                var floatArray = new List<float>();
                foreach (var value in values)
                {
                    floatArray.Add((float)value);
                }

                return floatArray.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Google AI: {ex.Message}");
                return null;
            }
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

                    services.AddDbContext<EmbeddingDbContext>(options =>
                    {
                        options.UseNpgsql(connectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.UseVector();
                        });
                    });

                    services.AddHttpClient();
                });
    }

    public class EmbeddingDbContext : DbContext
    {
        public EmbeddingDbContext(DbContextOptions<EmbeddingDbContext> options) : base(options) { }
        
        public DbSet<ExerciseRecord> Exercises => Set<ExerciseRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<ExerciseRecord>(entity =>
            {
                entity.ToTable("exercise_library");
                entity.HasKey(x => x.Id);
            });
        }
    }

    [Table("exercise_library")]
    public class ExerciseRecord
    {
        public Guid Id { get; set; }
        public string ExerciseName { get; set; } = "";
        public string ExerciseNameVi { get; set; } = "";
        public string Category { get; set; } = "";
        public string DifficultyLevel { get; set; } = "";
        [Column(TypeName = "jsonb")]
        public string EquipmentRequired { get; set; } = "";
        [Column(TypeName = "jsonb")]
        public string TargetMuscles { get; set; } = "";
        [Column(TypeName = "text")]
        public string Description { get; set; } = "";
        [Column(TypeName = "text")]
        public string DescriptionVi { get; set; } = "";
        [Column(TypeName = "vector(768)")]
        public Vector? EmbeddingVector { get; set; }
    }
}

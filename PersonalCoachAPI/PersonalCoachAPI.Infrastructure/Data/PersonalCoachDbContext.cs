using Microsoft.EntityFrameworkCore;
using PersonalCoachAPI.Domain.Entities;
using Pgvector.EntityFrameworkCore;

namespace PersonalCoachAPI.Infrastructure.Data
{
    public class PersonalCoachDbContext : DbContext
    {
        public PersonalCoachDbContext(DbContextOptions<PersonalCoachDbContext> options) : base(options)
        {
        }

        // PersonalCoach tables
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<UserHealthMetric> UserHealthMetrics => Set<UserHealthMetric>();
        public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
        public DbSet<WorkoutSchedule> WorkoutSchedules => Set<WorkoutSchedule>();
        public DbSet<WorkoutScheduleDay> WorkoutScheduleDays => Set<WorkoutScheduleDay>();
        public DbSet<WorkoutScheduleExercise> WorkoutScheduleExercises => Set<WorkoutScheduleExercise>();
        public DbSet<WorkoutFeedback> WorkoutFeedbacks => Set<WorkoutFeedback>();
        public DbSet<AIGenerationUsage> AIGenerationUsages => Set<AIGenerationUsage>();
        public DbSet<WorkoutTemplate> WorkoutTemplates => Set<WorkoutTemplate>();
        public DbSet<WorkoutTemplateExercise> WorkoutTemplateExercises => Set<WorkoutTemplateExercise>();
        public DbSet<ScheduleEditHistory> ScheduleEditHistories => Set<ScheduleEditHistory>();
        public DbSet<ExerciseLibrary> ExerciseLibrary => Set<ExerciseLibrary>();
        public DbSet<HealthKnowledgeBase> HealthKnowledgeBase => Set<HealthKnowledgeBase>();
        public DbSet<UserContextSnapshot> UserContextSnapshots => Set<UserContextSnapshot>();
        public DbSet<AIConversation> AIConversations => Set<AIConversation>();
        
        // Shared tables from other services (read-only access)
        public DbSet<AuthUser> Users => Set<AuthUser>();
        public DbSet<BodyMeasurement> BodyMeasurements => Set<BodyMeasurement>();
        public DbSet<OCRRequest> OCRRequests => Set<OCRRequest>();
        public DbSet<ProcessedData> ProcessedDataRecords => Set<ProcessedData>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("personalcoach");

            // Enable pgvector extension
            modelBuilder.HasPostgresExtension("vector");

            // Configure shared entities (read-only, no migrations)
            ConfigureSharedEntities(modelBuilder);

            // UserProfile configuration
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("user_profiles");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.FullName).HasMaxLength(200);
                entity.Property(x => x.Gender).HasMaxLength(20);
                entity.Property(x => x.FitnessLevel).HasMaxLength(50);
                entity.Property(x => x.FitnessGoal).HasMaxLength(100);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.HasIndex(x => x.UserId);
            });

            // UserHealthMetric configuration
            modelBuilder.Entity<UserHealthMetric>(entity =>
            {
                entity.ToTable("user_health_metrics");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Source).HasMaxLength(50);
                entity.Property(x => x.RawDataJson).HasColumnType("jsonb");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.MeasurementDate);
            });

            // UserPreference configuration
            modelBuilder.Entity<UserPreference>(entity =>
            {
                entity.ToTable("user_preferences");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.AvailableDays).HasColumnType("jsonb");
                entity.Property(x => x.PreferredWorkoutTime).HasMaxLength(50);
                entity.Property(x => x.EquipmentAvailable).HasColumnType("jsonb");
                entity.Property(x => x.DietaryRestrictions).HasColumnType("jsonb");
                entity.Property(x => x.DislikedExercises).HasColumnType("jsonb");
                entity.Property(x => x.FavoriteExercises).HasColumnType("jsonb");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.HasIndex(x => x.UserId);
            });

            // WorkoutSchedule configuration
            modelBuilder.Entity<WorkoutSchedule>(entity =>
            {
                entity.ToTable("workout_schedules");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ScheduleName).HasMaxLength(200);
                entity.Property(x => x.Goal).HasMaxLength(200);
                entity.Property(x => x.DifficultyLevel).HasMaxLength(50);
                entity.Property(x => x.Status).HasMaxLength(50);
                entity.Property(x => x.AiGenerationPrompt).HasColumnType("text");
                entity.Property(x => x.RegenerationReason).HasMaxLength(100);
                entity.Property(x => x.Version).HasColumnName("version");
                entity.Property(x => x.ParentScheduleId).HasColumnName("parent_schedule_id");
                entity.Property(x => x.RegeneratedFromDay).HasColumnName("regenerated_from_day");
                entity.Property(x => x.RegenerationReason).HasColumnName("regeneration_reason");
                entity.Property(x => x.IsLatest).HasColumnName("is_latest");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.Status);
                entity.HasIndex(x => new { x.UserId, x.IsLatest });
                entity.HasIndex(x => x.ParentScheduleId);
            });

            // WorkoutScheduleDay configuration
            modelBuilder.Entity<WorkoutScheduleDay>(entity =>
            {
                entity.ToTable("workout_schedule_days");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.DayType).HasMaxLength(50);
                entity.Property(x => x.FocusArea).HasMaxLength(100);
                entity.Property(x => x.Status).HasMaxLength(50);
                entity.Property(x => x.UserNotes).HasColumnType("text");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasOne(x => x.Schedule)
                    .WithMany(x => x.ScheduleDays)
                    .HasForeignKey(x => x.ScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(x => x.ScheduleId);
                entity.HasIndex(x => new { x.ScheduleId, x.DayNumber });
            });

            // WorkoutScheduleExercise configuration
            modelBuilder.Entity<WorkoutScheduleExercise>(entity =>
            {
                entity.ToTable("workout_schedule_exercises");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ExerciseName).IsRequired().HasMaxLength(200);
                entity.Property(x => x.ExerciseCategory).HasMaxLength(100);
                entity.Property(x => x.TargetMuscles).HasColumnType("jsonb");
                entity.Property(x => x.Reps).HasMaxLength(50);
                entity.Property(x => x.Notes).HasColumnType("text");
                entity.Property(x => x.VideoUrl).HasMaxLength(500);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasOne(x => x.ScheduleDay)
                    .WithMany(x => x.Exercises)
                    .HasForeignKey(x => x.ScheduleDayId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(x => x.ScheduleDayId);
                entity.HasIndex(x => x.OrderIndex);
            });

            // ExerciseLibrary configuration with pgvector
            modelBuilder.Entity<ExerciseLibrary>(entity =>
            {
                entity.ToTable("exercise_library");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.ExerciseName).IsRequired().HasMaxLength(200);
                entity.Property(x => x.ExerciseNameVi).HasMaxLength(200);
                entity.Property(x => x.Category).HasMaxLength(100);
                entity.Property(x => x.DifficultyLevel).HasMaxLength(50);
                entity.Property(x => x.EquipmentRequired).HasColumnType("jsonb");
                entity.Property(x => x.TargetMuscles).HasColumnType("jsonb");
                entity.Property(x => x.SecondaryMuscles).HasColumnType("jsonb");
                entity.Property(x => x.Description).HasColumnType("text");
                entity.Property(x => x.DescriptionVi).HasColumnType("text");
                entity.Property(x => x.Instructions).HasColumnType("jsonb");
                entity.Property(x => x.Benefits).HasColumnType("jsonb");
                entity.Property(x => x.CommonMistakes).HasColumnType("jsonb");
                entity.Property(x => x.Variations).HasColumnType("jsonb");
                entity.Property(x => x.VideoUrl).HasMaxLength(500);
                entity.Property(x => x.ImageUrl).HasMaxLength(500);
                entity.Property(x => x.EmbeddingVector).HasColumnType("vector(768)"); // Google text-embedding-004 size
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasIndex(x => x.Category);
                entity.HasIndex(x => x.DifficultyLevel);
                // Vector index for fast similarity search
                entity.HasIndex(x => x.EmbeddingVector).HasMethod("ivfflat");
            });

            // HealthKnowledgeBase configuration with pgvector
            modelBuilder.Entity<HealthKnowledgeBase>(entity =>
            {
                entity.ToTable("health_knowledge_base");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Category).HasMaxLength(100);
                entity.Property(x => x.Topic).HasMaxLength(200);
                entity.Property(x => x.Content).HasColumnType("text");
                entity.Property(x => x.ContentVi).HasColumnType("text");
                entity.Property(x => x.Source).HasMaxLength(500);
                entity.Property(x => x.Tags).HasColumnType("jsonb");
                entity.Property(x => x.EmbeddingVector).HasColumnType("vector(768)"); // Google text-embedding-004 size
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasIndex(x => x.Category);
                entity.HasIndex(x => x.EmbeddingVector).HasMethod("ivfflat");
            });

            // UserContextSnapshot configuration with pgvector
            modelBuilder.Entity<UserContextSnapshot>(entity =>
            {
                entity.ToTable("user_context_snapshots");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.HealthSummary).HasColumnType("text");
                entity.Property(x => x.FitnessSummary).HasColumnType("text");
                entity.Property(x => x.GoalsSummary).HasColumnType("text");
                entity.Property(x => x.ProgressSummary).HasColumnType("text");
                entity.Property(x => x.EmbeddingVector).HasColumnType("vector(768)"); // Google text-embedding-004 size
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.SnapshotDate);
                entity.HasIndex(x => x.EmbeddingVector).HasMethod("ivfflat");
            });

            // WorkoutFeedback configuration
            modelBuilder.Entity<WorkoutFeedback>(entity =>
            {
                entity.ToTable("workout_feedbacks");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.FeedbackType).HasMaxLength(50);
                entity.Property(x => x.DifficultyRating).HasMaxLength(50);
                entity.Property(x => x.ExerciseFeedback).HasMaxLength(100);
                entity.Property(x => x.Notes).HasColumnType("text");
                entity.Property(x => x.ContextTags).HasColumnType("jsonb");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasOne(x => x.Schedule)
                    .WithMany(x => x.Feedbacks)
                    .HasForeignKey(x => x.ScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.ScheduleId);
                entity.HasIndex(x => x.ScheduleDayId);
                entity.HasIndex(x => x.DifficultyRating);
            });

            // ScheduleEditHistory configuration
            modelBuilder.Entity<ScheduleEditHistory>(entity =>
            {
                entity.ToTable("schedule_edit_history");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.UserId).HasColumnName("user_id");
                entity.Property(x => x.ScheduleId).HasColumnName("schedule_id");
                entity.Property(x => x.ScheduleDayId).HasColumnName("schedule_day_id");
                entity.Property(x => x.EditType).HasMaxLength(50).HasColumnName("edit_type");
                entity.Property(x => x.EditDescription).HasMaxLength(500).HasColumnName("edit_description");
                entity.Property(x => x.BeforeValue).HasColumnType("jsonb").HasColumnName("before_value");
                entity.Property(x => x.AfterValue).HasColumnType("jsonb").HasColumnName("after_value");
                entity.Property(x => x.EditSource).HasMaxLength(50).HasColumnName("edit_source");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'").HasColumnName("created_at");
                
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.ScheduleId);
                entity.HasIndex(x => x.EditType);
                entity.HasIndex(x => x.CreatedAt);
            });

            // AIGenerationUsage configuration
            modelBuilder.Entity<AIGenerationUsage>(entity =>
            {
                entity.ToTable("ai_generation_usage");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.GenerationType).HasMaxLength(50).IsRequired();
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => new { x.UserId, x.Year, x.Month });
            });

            // WorkoutTemplate configuration
            modelBuilder.Entity<WorkoutTemplate>(entity =>
            {
                entity.ToTable("workout_templates");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TemplateName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.FocusArea).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Difficulty).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Description).HasColumnType("text");
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("now() at time zone 'utc'");
                
                entity.HasIndex(x => x.FocusArea);
                entity.HasIndex(x => x.Difficulty);
                entity.HasIndex(x => x.IsActive);
            });

            // WorkoutTemplateExercise configuration
            modelBuilder.Entity<WorkoutTemplateExercise>(entity =>
            {
                entity.ToTable("workout_template_exercises");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Reps).HasMaxLength(50);
                entity.Property(x => x.Notes).HasColumnType("text");
                
                entity.HasOne(x => x.Template)
                    .WithMany(x => x.Exercises)
                    .HasForeignKey(x => x.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(x => x.Exercise)
                    .WithMany()
                    .HasForeignKey(x => x.ExerciseLibraryId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasIndex(x => x.TemplateId);
            });
        }

        private static void ConfigureSharedEntities(ModelBuilder modelBuilder)
        {
            // AuthUser - from AuthenticationAPI (read-only, exclude from migrations)
            modelBuilder.Entity<AuthUser>(entity =>
            {
                entity.ToTable("users", "authentication", t => t.ExcludeFromMigrations());
                entity.HasKey(x => x.Id);
            });

            // BodyMeasurement - from MeasurementAPI (read-only, exclude from migrations)
            modelBuilder.Entity<BodyMeasurement>(entity =>
            {
                entity.ToTable("body_measurements", "measurement", t => t.ExcludeFromMigrations());
                entity.HasKey(x => x.Id);
            });

            // OCRRequest - from AIOCRService (read-only, exclude from migrations)
            modelBuilder.Entity<OCRRequest>(entity =>
            {
                entity.ToTable("ocr_requests", "aiocr", t => t.ExcludeFromMigrations());
                entity.HasKey(x => x.Id);
            });

            // ProcessedData - from AIOCRService (read-only, exclude from migrations)
            modelBuilder.Entity<ProcessedData>(entity =>
            {
                entity.ToTable("processed_data", "aiocr", t => t.ExcludeFromMigrations());
                entity.HasKey(x => x.Id);
            });

            // AIConversation - shared table (read-only, exclude from migrations)
            modelBuilder.Entity<AIConversation>(entity =>
            {
                entity.ToTable("ai_conversations", t => t.ExcludeFromMigrations());
                entity.HasKey(x => x.Id);
            });
        }
    }
}


using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersonalCoach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            // Use raw SQL with IF NOT EXISTS to make migration idempotent
            // This prevents "relation already exists" errors when tables are created by other scripts
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.exercise_library (
                    ""Id"" uuid NOT NULL,
                    ""ExerciseName"" character varying(200) NOT NULL,
                    ""ExerciseNameVi"" character varying(200) NOT NULL,
                    ""Category"" character varying(100) NOT NULL,
                    ""DifficultyLevel"" character varying(50) NOT NULL,
                    ""EquipmentRequired"" jsonb NOT NULL,
                    ""TargetMuscles"" jsonb NOT NULL,
                    ""SecondaryMuscles"" jsonb NOT NULL,
                    ""Description"" text NOT NULL,
                    ""DescriptionVi"" text NOT NULL,
                    ""Instructions"" jsonb NOT NULL,
                    ""Benefits"" jsonb NOT NULL,
                    ""CommonMistakes"" jsonb NOT NULL,
                    ""Variations"" jsonb NOT NULL,
                    ""VideoUrl"" character varying(500) NOT NULL,
                    ""ImageUrl"" character varying(500) NOT NULL,
                    ""CaloriesPerMinute"" numeric NOT NULL,
                    ""EmbeddingVector"" vector(768),
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    ""UpdatedAt"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_exercise_library"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.health_knowledge_base (
                    ""Id"" uuid NOT NULL,
                    ""Category"" character varying(100) NOT NULL,
                    ""Topic"" character varying(200) NOT NULL,
                    ""Content"" text NOT NULL,
                    ""ContentVi"" text NOT NULL,
                    ""Source"" character varying(500) NOT NULL,
                    ""ReliabilityScore"" numeric NOT NULL,
                    ""Tags"" jsonb NOT NULL,
                    ""EmbeddingVector"" vector(768),
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    CONSTRAINT ""PK_health_knowledge_base"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.schedule_edit_history (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    schedule_id uuid NOT NULL,
                    schedule_day_id uuid,
                    edit_type character varying(50) NOT NULL,
                    edit_description character varying(500) NOT NULL,
                    before_value jsonb,
                    after_value jsonb,
                    edit_source character varying(50) NOT NULL,
                    created_at timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    CONSTRAINT ""PK_schedule_edit_history"" PRIMARY KEY (id)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.user_context_snapshots (
                    ""Id"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""SnapshotDate"" timestamp with time zone NOT NULL,
                    ""HealthSummary"" text NOT NULL,
                    ""FitnessSummary"" text NOT NULL,
                    ""GoalsSummary"" text NOT NULL,
                    ""ProgressSummary"" text NOT NULL,
                    ""EmbeddingVector"" vector(768),
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    CONSTRAINT ""PK_user_context_snapshots"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.user_schedules (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    schedule_name character varying(200) NOT NULL,
                    description text,
                    schedule_type character varying(50) NOT NULL,
                    start_date timestamp with time zone NOT NULL,
                    end_date timestamp with time zone,
                    frequency_per_week integer NOT NULL,
                    total_weeks integer,
                    is_active boolean NOT NULL,
                    ai_generated boolean NOT NULL,
                    difficulty_level character varying(50),
                    tags jsonb,
                    metadata jsonb,
                    created_at timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_user_schedules"" PRIMARY KEY (id)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.schedule_days (
                    id uuid NOT NULL,
                    schedule_id uuid NOT NULL,
                    day_of_week integer NOT NULL,
                    week_number integer NOT NULL,
                    is_rest_day boolean NOT NULL,
                    focus_area character varying(100),
                    total_duration_minutes integer,
                    notes text,
                    created_at timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_schedule_days"" PRIMARY KEY (id)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.schedule_exercises (
                    id uuid NOT NULL,
                    schedule_day_id uuid NOT NULL,
                    exercise_id uuid NOT NULL,
                    exercise_order integer NOT NULL,
                    sets integer NOT NULL,
                    reps integer,
                    duration_minutes integer,
                    rest_seconds integer,
                    intensity_level character varying(50),
                    notes text,
                    created_at timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    updated_at timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_schedule_exercises"" PRIMARY KEY (id)
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.user_health_metrics (
                    ""Id"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""MeasurementDate"" timestamp with time zone NOT NULL,
                    ""WeightKg"" numeric NOT NULL,
                    ""MuscleMassKg"" numeric NOT NULL,
                    ""BodyFatPercentage"" numeric NOT NULL,
                    ""BodyFatKg"" numeric NOT NULL,
                    ""BmrKcal"" integer NOT NULL,
                    ""WaterPercentage"" numeric NOT NULL,
                    ""BoneMassKg"" numeric NOT NULL,
                    ""VisceralFatLevel"" integer NOT NULL,
                    ""MetabolicAge"" integer NOT NULL,
                    ""Source"" character varying(50) NOT NULL,
                    ""RawDataJson"" jsonb NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    CONSTRAINT ""PK_user_health_metrics"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.user_preferences (
                    ""Id"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""AvailableDays"" jsonb NOT NULL,
                    ""PreferredWorkoutTime"" character varying(50) NOT NULL,
                    ""WorkoutDurationMinutes"" integer NOT NULL,
                    ""EquipmentAvailable"" jsonb NOT NULL,
                    ""GymAccess"" boolean NOT NULL,
                    ""HomeWorkoutOnly"" boolean NOT NULL,
                    ""DietaryRestrictions"" jsonb NOT NULL,
                    ""DislikedExercises"" jsonb NOT NULL,
                    ""FavoriteExercises"" jsonb NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    ""UpdatedAt"" timestamp with time zone,
                    CONSTRAINT ""PK_user_preferences"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.user_profiles (
                    ""Id"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""FullName"" character varying(200) NOT NULL,
                    ""DateOfBirth"" timestamp with time zone NOT NULL,
                    ""Gender"" character varying(20) NOT NULL,
                    ""HeightCm"" numeric NOT NULL,
                    ""TargetWeightKg"" numeric,
                    ""FitnessGoals"" jsonb NOT NULL,
                    ""ActivityLevel"" character varying(50) NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    ""UpdatedAt"" timestamp with time zone,
                    CONSTRAINT ""PK_user_profiles"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.workout_schedules (
                    ""Id"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""GoalType"" character varying(100) NOT NULL,
                    ""GeneratedAt"" timestamp with time zone NOT NULL,
                    ""StartDate"" timestamp with time zone NOT NULL,
                    ""EndDate"" timestamp with time zone NOT NULL,
                    ""Status"" character varying(50) NOT NULL,
                    is_latest boolean NOT NULL,
                    parent_schedule_id uuid,
                    regeneration_reason text,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    CONSTRAINT ""PK_workout_schedules"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.workout_schedule_days (
                    ""Id"" uuid NOT NULL,
                    ""ScheduleId"" uuid NOT NULL,
                    ""DayNumber"" integer NOT NULL,
                    ""DayOfWeek"" integer NOT NULL,
                    ""FocusArea"" character varying(100) NOT NULL,
                    ""Description"" text,
                    is_rest_day boolean NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    CONSTRAINT ""PK_workout_schedule_days"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.workout_feedbacks (
                    ""Id"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""ScheduleId"" uuid NOT NULL,
                    ""ScheduleDayId"" uuid,
                    ""FeedbackDate"" timestamp with time zone NOT NULL,
                    ""DifficultyRating"" integer NOT NULL,
                    ""EnjoymentRating"" integer NOT NULL,
                    ""CompletionRate"" numeric NOT NULL,
                    ""Comments"" text,
                    ""EnergyLevel"" character varying(50),
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    CONSTRAINT ""PK_workout_feedbacks"" PRIMARY KEY (""Id"")
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS personalcoach.workout_schedule_exercises (
                    ""Id"" uuid NOT NULL,
                    ""ScheduleDayId"" uuid NOT NULL,
                    ""ExerciseName"" character varying(200) NOT NULL,
                    ""ExerciseNameVi"" character varying(200) NOT NULL,
                    ""OrderIndex"" integer NOT NULL,
                    ""Sets"" integer NOT NULL,
                    ""Reps"" character varying(50) NOT NULL,
                    ""Duration"" character varying(50) NOT NULL,
                    ""RestBetweenSets"" integer NOT NULL,
                    ""Instructions"" text NOT NULL,
                    ""VideoUrl"" character varying(500) NOT NULL,
                    ""Completed"" boolean NOT NULL,
                    ""ActualSets"" integer,
                    ""ActualReps"" integer,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
                    CONSTRAINT ""PK_workout_schedule_exercises"" PRIMARY KEY (""Id"")
                );
            ");

            // Create indexes if they don't exist
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_exercise_library_Category ON personalcoach.exercise_library (""Category"");
                CREATE INDEX IF NOT EXISTS IX_exercise_library_DifficultyLevel ON personalcoach.exercise_library (""DifficultyLevel"");
                CREATE INDEX IF NOT EXISTS IX_health_knowledge_base_Category ON personalcoach.health_knowledge_base (""Category"");
                CREATE INDEX IF NOT EXISTS IX_schedule_edit_history_created_at ON personalcoach.schedule_edit_history (created_at);
                CREATE INDEX IF NOT EXISTS IX_schedule_edit_history_edit_type ON personalcoach.schedule_edit_history (edit_type);
                CREATE INDEX IF NOT EXISTS IX_schedule_edit_history_schedule_id ON personalcoach.schedule_edit_history (schedule_id);
                CREATE INDEX IF NOT EXISTS IX_schedule_edit_history_user_id ON personalcoach.schedule_edit_history (user_id);
                CREATE INDEX IF NOT EXISTS IX_user_context_snapshots_SnapshotDate ON personalcoach.user_context_snapshots (""SnapshotDate"");
                CREATE INDEX IF NOT EXISTS IX_user_context_snapshots_UserId ON personalcoach.user_context_snapshots (""UserId"");
                CREATE INDEX IF NOT EXISTS IX_user_health_metrics_MeasurementDate ON personalcoach.user_health_metrics (""MeasurementDate"");
                CREATE INDEX IF NOT EXISTS IX_user_health_metrics_UserId ON personalcoach.user_health_metrics (""UserId"");
                CREATE INDEX IF NOT EXISTS IX_user_preferences_UserId ON personalcoach.user_preferences (""UserId"");
                CREATE INDEX IF NOT EXISTS IX_user_profiles_UserId ON personalcoach.user_profiles (""UserId"");
                CREATE INDEX IF NOT EXISTS IX_workout_feedbacks_DifficultyRating ON personalcoach.workout_feedbacks (""DifficultyRating"");
                CREATE INDEX IF NOT EXISTS IX_workout_feedbacks_ScheduleDayId ON personalcoach.workout_feedbacks (""ScheduleDayId"");
                CREATE INDEX IF NOT EXISTS IX_workout_feedbacks_ScheduleId ON personalcoach.workout_feedbacks (""ScheduleId"");
                CREATE INDEX IF NOT EXISTS IX_workout_feedbacks_UserId ON personalcoach.workout_feedbacks (""UserId"");
                CREATE INDEX IF NOT EXISTS IX_workout_schedule_days_ScheduleId ON personalcoach.workout_schedule_days (""ScheduleId"");
                CREATE INDEX IF NOT EXISTS IX_workout_schedule_exercises_OrderIndex ON personalcoach.workout_schedule_exercises (""OrderIndex"");
                CREATE INDEX IF NOT EXISTS IX_workout_schedule_exercises_ScheduleDayId ON personalcoach.workout_schedule_exercises (""ScheduleDayId"");
                CREATE INDEX IF NOT EXISTS IX_workout_schedules_parent_schedule_id ON personalcoach.workout_schedules (parent_schedule_id);
                CREATE INDEX IF NOT EXISTS IX_workout_schedules_Status ON personalcoach.workout_schedules (""Status"");
                CREATE INDEX IF NOT EXISTS IX_workout_schedules_UserId ON personalcoach.workout_schedules (""UserId"");
            ");

            // Create vector indexes (special ivfflat method for embeddings)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_exercise_library_EmbeddingVector ON personalcoach.exercise_library USING ivfflat (""EmbeddingVector"" vector_cosine_ops);
                CREATE INDEX IF NOT EXISTS IX_health_knowledge_base_EmbeddingVector ON personalcoach.health_knowledge_base USING ivfflat (""EmbeddingVector"" vector_cosine_ops);
                CREATE INDEX IF NOT EXISTS IX_user_context_snapshots_EmbeddingVector ON personalcoach.user_context_snapshots USING ivfflat (""EmbeddingVector"" vector_cosine_ops);
            ");

            // Create composite indexes
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_workout_schedule_days_ScheduleId_DayNumber ON personalcoach.workout_schedule_days (""ScheduleId"", ""DayNumber"");
                CREATE INDEX IF NOT EXISTS IX_workout_schedules_UserId_is_latest ON personalcoach.workout_schedules (""UserId"", is_latest);
            ");

            // Create foreign keys if they don't exist
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    -- workout_schedule_days -> workout_schedules
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'FK_workout_schedule_days_workout_schedules_ScheduleId'
                    ) THEN
                        ALTER TABLE personalcoach.workout_schedule_days 
                        ADD CONSTRAINT ""FK_workout_schedule_days_workout_schedules_ScheduleId"" 
                        FOREIGN KEY (""ScheduleId"") REFERENCES personalcoach.workout_schedules(""Id"") ON DELETE CASCADE;
                    END IF;

                    -- workout_schedule_exercises -> workout_schedule_days
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'FK_workout_schedule_exercises_workout_schedule_days_ScheduleDa~'
                    ) THEN
                        ALTER TABLE personalcoach.workout_schedule_exercises 
                        ADD CONSTRAINT ""FK_workout_schedule_exercises_workout_schedule_days_ScheduleDa~"" 
                        FOREIGN KEY (""ScheduleDayId"") REFERENCES personalcoach.workout_schedule_days(""Id"") ON DELETE CASCADE;
                    END IF;
                END $$;
            ");

            // Keep original EF Core code commented for reference
            /*migrationBuilder.CreateTable(
                name: "exercise_library",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExerciseNameVi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DifficultyLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EquipmentRequired = table.Column<string>(type: "jsonb", nullable: false),
                    TargetMuscles = table.Column<string>(type: "jsonb", nullable: false),
                    SecondaryMuscles = table.Column<string>(type: "jsonb", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DescriptionVi = table.Column<string>(type: "text", nullable: false),
                    Instructions = table.Column<string>(type: "jsonb", nullable: false),
                    Benefits = table.Column<string>(type: "jsonb", nullable: false),
                    CommonMistakes = table.Column<string>(type: "jsonb", nullable: false),
                    Variations = table.Column<string>(type: "jsonb", nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CaloriesPerMinute = table.Column<decimal>(type: "numeric", nullable: false),
                    EmbeddingVector = table.Column<Vector>(type: "vector(768)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_library", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "health_knowledge_base",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ContentVi = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReliabilityScore = table.Column<decimal>(type: "numeric", nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    EmbeddingVector = table.Column<Vector>(type: "vector(768)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_knowledge_base", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "schedule_edit_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schedule_day_id = table.Column<Guid>(type: "uuid", nullable: true),
                    edit_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    edit_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    before_value = table.Column<string>(type: "jsonb", nullable: true),
                    after_value = table.Column<string>(type: "jsonb", nullable: true),
                    edit_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_edit_history", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_context_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HealthSummary = table.Column<string>(type: "text", nullable: false),
                    FitnessSummary = table.Column<string>(type: "text", nullable: false),
                    GoalsSummary = table.Column<string>(type: "text", nullable: false),
                    ProgressSummary = table.Column<string>(type: "text", nullable: false),
                    EmbeddingVector = table.Column<Vector>(type: "vector(768)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_context_snapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_health_metrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: false),
                    MuscleMassKg = table.Column<decimal>(type: "numeric", nullable: false),
                    BodyFatPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    BodyFatKg = table.Column<decimal>(type: "numeric", nullable: false),
                    BmrKcal = table.Column<int>(type: "integer", nullable: false),
                    WaterPercentage = table.Column<decimal>(type: "numeric", nullable: false),
                    BoneMassKg = table.Column<decimal>(type: "numeric", nullable: false),
                    VisceralFatLevel = table.Column<int>(type: "integer", nullable: false),
                    MetabolicAge = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RawDataJson = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_health_metrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_preferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AvailableDays = table.Column<string>(type: "jsonb", nullable: false),
                    PreferredWorkoutTime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkoutDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    EquipmentAvailable = table.Column<string>(type: "jsonb", nullable: false),
                    GymAccess = table.Column<bool>(type: "boolean", nullable: false),
                    HomeWorkoutOnly = table.Column<bool>(type: "boolean", nullable: false),
                    DietaryRestrictions = table.Column<string>(type: "jsonb", nullable: false),
                    DislikedExercises = table.Column<string>(type: "jsonb", nullable: false),
                    FavoriteExercises = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_preferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HeightCm = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentWeightKg = table.Column<decimal>(type: "numeric", nullable: false),
                    TargetWeightKg = table.Column<decimal>(type: "numeric", nullable: false),
                    FitnessLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FitnessGoal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workout_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Goal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DifficultyLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AiGenerationPrompt = table.Column<string>(type: "text", nullable: false),
                    CreatedByAi = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    parent_schedule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    regenerated_from_day = table.Column<int>(type: "integer", nullable: true),
                    regeneration_reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_latest = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_schedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workout_schedule_days",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DayType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FocusArea = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EstimatedDuration = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserNotes = table.Column<string>(type: "text", nullable: false),
                    AiModified = table.Column<bool>(type: "boolean", nullable: false),
                    ModificationCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_schedule_days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_schedule_days_workout_schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "workout_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_feedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    FeedbackType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DifficultyRating = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EnergyLevel = table.Column<int>(type: "integer", nullable: true),
                    SorenessLevel = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    TookLongerThanExpected = table.Column<bool>(type: "boolean", nullable: true),
                    ActualDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExerciseFeedback = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContextTags = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_feedbacks_workout_schedule_days_ScheduleDayId",
                        column: x => x.ScheduleDayId,
                        principalTable: "workout_schedule_days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workout_feedbacks_workout_schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "workout_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workout_schedule_exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleDayId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExerciseCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TargetMuscles = table.Column<string>(type: "jsonb", nullable: false),
                    Sets = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    RestSeconds = table.Column<int>(type: "integer", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    ActualSets = table.Column<int>(type: "integer", nullable: true),
                    ActualReps = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_schedule_exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_schedule_exercises_workout_schedule_days_ScheduleDa~",
                        column: x => x.ScheduleDayId,
                        principalTable: "workout_schedule_days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            */

            // Use raw SQL for additional indexes (already handled above with CREATE INDEX IF NOT EXISTS)
            // Original EF Core CreateIndex calls commented out below:
            /*
            migrationBuilder.CreateIndex(
                name: "IX_exercise_library_Category",
                table: "exercise_library",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_library_DifficultyLevel",
                table: "exercise_library",
                column: "DifficultyLevel");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_library_EmbeddingVector",
                table: "exercise_library",
                column: "EmbeddingVector")
                .Annotation("Npgsql:IndexMethod", "ivfflat");

            migrationBuilder.CreateIndex(
                name: "IX_health_knowledge_base_Category",
                table: "health_knowledge_base",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_health_knowledge_base_EmbeddingVector",
                table: "health_knowledge_base",
                column: "EmbeddingVector")
                .Annotation("Npgsql:IndexMethod", "ivfflat");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_edit_history_created_at",
                table: "schedule_edit_history",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_edit_history_edit_type",
                table: "schedule_edit_history",
                column: "edit_type");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_edit_history_schedule_id",
                table: "schedule_edit_history",
                column: "schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_edit_history_user_id",
                table: "schedule_edit_history",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_context_snapshots_EmbeddingVector",
                table: "user_context_snapshots",
                column: "EmbeddingVector")
                .Annotation("Npgsql:IndexMethod", "ivfflat");

            migrationBuilder.CreateIndex(
                name: "IX_user_context_snapshots_SnapshotDate",
                table: "user_context_snapshots",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_user_context_snapshots_UserId",
                table: "user_context_snapshots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_health_metrics_MeasurementDate",
                table: "user_health_metrics",
                column: "MeasurementDate");

            migrationBuilder.CreateIndex(
                name: "IX_user_health_metrics_UserId",
                table: "user_health_metrics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_preferences_UserId",
                table: "user_preferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_UserId",
                table: "user_profiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_feedbacks_DifficultyRating",
                table: "workout_feedbacks",
                column: "DifficultyRating");

            migrationBuilder.CreateIndex(
                name: "IX_workout_feedbacks_ScheduleDayId",
                table: "workout_feedbacks",
                column: "ScheduleDayId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_feedbacks_ScheduleId",
                table: "workout_feedbacks",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_feedbacks_UserId",
                table: "workout_feedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedule_days_ScheduleId",
                table: "workout_schedule_days",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedule_days_ScheduleId_DayNumber",
                table: "workout_schedule_days",
                columns: new[] { "ScheduleId", "DayNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedule_exercises_OrderIndex",
                table: "workout_schedule_exercises",
                column: "OrderIndex");

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedule_exercises_ScheduleDayId",
                table: "workout_schedule_exercises",
                column: "ScheduleDayId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedules_parent_schedule_id",
                table: "workout_schedules",
                column: "parent_schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedules_Status",
                table: "workout_schedules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedules_UserId",
                table: "workout_schedules",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedules_UserId_is_latest",
                table: "workout_schedules",
                columns: new[] { "UserId", "is_latest" });
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exercise_library");

            migrationBuilder.DropTable(
                name: "health_knowledge_base");

            migrationBuilder.DropTable(
                name: "schedule_edit_history");

            migrationBuilder.DropTable(
                name: "user_context_snapshots");

            migrationBuilder.DropTable(
                name: "user_health_metrics");

            migrationBuilder.DropTable(
                name: "user_preferences");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "workout_feedbacks");

            migrationBuilder.DropTable(
                name: "workout_schedule_exercises");

            migrationBuilder.DropTable(
                name: "workout_schedule_days");

            migrationBuilder.DropTable(
                name: "workout_schedules");
        }
    }
}

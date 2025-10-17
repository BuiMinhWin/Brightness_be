using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetPersonalCoachSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "personalcoach");

            // Use raw SQL to conditionally move tables from public to personalcoach schema
            // This is idempotent and won't fail if tables are already in personalcoach schema
            var tables = new[] {
                "workout_schedules",
                "workout_schedule_exercises",
                "workout_schedule_days",
                "workout_feedbacks",
                "user_profiles",
                "user_preferences",
                "user_health_metrics",
                "user_context_snapshots",
                "schedule_edit_history",
                "health_knowledge_base",
                "exercise_library"
            };

            foreach (var table in tables)
            {
                migrationBuilder.Sql($@"
                    DO $$
                    BEGIN
                        -- Only move table if it exists in public schema and not in personalcoach schema
                        IF EXISTS (
                            SELECT 1 FROM pg_tables 
                            WHERE schemaname = 'public' AND tablename = '{table}'
                        ) AND NOT EXISTS (
                            SELECT 1 FROM pg_tables 
                            WHERE schemaname = 'personalcoach' AND tablename = '{table}'
                        ) THEN
                            ALTER TABLE public.{table} SET SCHEMA personalcoach;
                        END IF;
                    END $$;
                ");
            }

            /* Original EF Core RenameTable calls - replaced with idempotent SQL above
            migrationBuilder.RenameTable(
                name: "workout_schedules",
                newName: "workout_schedules",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "workout_schedule_exercises",
                newName: "workout_schedule_exercises",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "workout_schedule_days",
                newName: "workout_schedule_days",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "workout_feedbacks",
                newName: "workout_feedbacks",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "user_profiles",
                newName: "user_profiles",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "user_preferences",
                newName: "user_preferences",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "user_health_metrics",
                newName: "user_health_metrics",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "user_context_snapshots",
                newName: "user_context_snapshots",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "schedule_edit_history",
                newName: "schedule_edit_history",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "health_knowledge_base",
                newName: "health_knowledge_base",
                newSchema: "personalcoach");

            migrationBuilder.RenameTable(
                name: "exercise_library",
                newName: "exercise_library",
                newSchema: "personalcoach");
            */
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "workout_schedules",
                schema: "personalcoach",
                newName: "workout_schedules");

            migrationBuilder.RenameTable(
                name: "workout_schedule_exercises",
                schema: "personalcoach",
                newName: "workout_schedule_exercises");

            migrationBuilder.RenameTable(
                name: "workout_schedule_days",
                schema: "personalcoach",
                newName: "workout_schedule_days");

            migrationBuilder.RenameTable(
                name: "workout_feedbacks",
                schema: "personalcoach",
                newName: "workout_feedbacks");

            migrationBuilder.RenameTable(
                name: "user_profiles",
                schema: "personalcoach",
                newName: "user_profiles");

            migrationBuilder.RenameTable(
                name: "user_preferences",
                schema: "personalcoach",
                newName: "user_preferences");

            migrationBuilder.RenameTable(
                name: "user_health_metrics",
                schema: "personalcoach",
                newName: "user_health_metrics");

            migrationBuilder.RenameTable(
                name: "user_context_snapshots",
                schema: "personalcoach",
                newName: "user_context_snapshots");

            migrationBuilder.RenameTable(
                name: "schedule_edit_history",
                schema: "personalcoach",
                newName: "schedule_edit_history");

            migrationBuilder.RenameTable(
                name: "health_knowledge_base",
                schema: "personalcoach",
                newName: "health_knowledge_base");

            migrationBuilder.RenameTable(
                name: "exercise_library",
                schema: "personalcoach",
                newName: "exercise_library");
        }
    }
}

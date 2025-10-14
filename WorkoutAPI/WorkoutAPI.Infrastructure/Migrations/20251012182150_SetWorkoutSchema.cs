using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkoutAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetWorkoutSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workout");

            migrationBuilder.RenameTable(
                name: "workout_sessions",
                newName: "workout_sessions",
                newSchema: "workout");

            migrationBuilder.RenameTable(
                name: "workout_plans",
                newName: "workout_plans",
                newSchema: "workout");

            migrationBuilder.RenameTable(
                name: "session_exercises",
                newName: "session_exercises",
                newSchema: "workout");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "workout_sessions",
                schema: "workout",
                newName: "workout_sessions");

            migrationBuilder.RenameTable(
                name: "workout_plans",
                schema: "workout",
                newName: "workout_plans");

            migrationBuilder.RenameTable(
                name: "session_exercises",
                schema: "workout",
                newName: "session_exercises");
        }
    }
}

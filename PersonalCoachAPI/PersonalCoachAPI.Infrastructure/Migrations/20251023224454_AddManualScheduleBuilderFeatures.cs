using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualScheduleBuilderFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "personalcoach",
                table: "exercise_library",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "CaloriesPerMinute",
                schema: "personalcoach",
                table: "exercise_library",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "personalcoach",
                table: "exercise_library",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ai_generation_usage",
                schema: "personalcoach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenerationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    RelatedScheduleId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_generation_usage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workout_templates",
                schema: "personalcoach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FocusArea = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstimatedDuration = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workout_template_exercises",
                schema: "personalcoach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseLibraryId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Sets = table.Column<int>(type: "integer", nullable: false),
                    Reps = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RestSeconds = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workout_template_exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workout_template_exercises_exercise_library_ExerciseLibrary~",
                        column: x => x.ExerciseLibraryId,
                        principalSchema: "personalcoach",
                        principalTable: "exercise_library",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workout_template_exercises_workout_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "personalcoach",
                        principalTable: "workout_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_generation_usage_UserId",
                schema: "personalcoach",
                table: "ai_generation_usage",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_generation_usage_UserId_Year_Month",
                schema: "personalcoach",
                table: "ai_generation_usage",
                columns: new[] { "UserId", "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_workout_template_exercises_ExerciseLibraryId",
                schema: "personalcoach",
                table: "workout_template_exercises",
                column: "ExerciseLibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_template_exercises_TemplateId",
                schema: "personalcoach",
                table: "workout_template_exercises",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_workout_templates_Difficulty",
                schema: "personalcoach",
                table: "workout_templates",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_workout_templates_FocusArea",
                schema: "personalcoach",
                table: "workout_templates",
                column: "FocusArea");

            migrationBuilder.CreateIndex(
                name: "IX_workout_templates_IsActive",
                schema: "personalcoach",
                table: "workout_templates",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_generation_usage",
                schema: "personalcoach");

            migrationBuilder.DropTable(
                name: "workout_template_exercises",
                schema: "personalcoach");

            migrationBuilder.DropTable(
                name: "workout_templates",
                schema: "personalcoach");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "personalcoach",
                table: "exercise_library");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                schema: "personalcoach",
                table: "exercise_library",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CaloriesPerMinute",
                schema: "personalcoach",
                table: "exercise_library",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}

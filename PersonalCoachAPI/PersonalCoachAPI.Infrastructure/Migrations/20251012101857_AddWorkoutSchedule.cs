using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_latest",
                table: "workout_schedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_schedule_id",
                table: "workout_schedules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "regenerated_from_day",
                table: "workout_schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "regeneration_reason",
                table: "workout_schedules",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "workout_schedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedules_parent_schedule_id",
                table: "workout_schedules",
                column: "parent_schedule_id");

            migrationBuilder.CreateIndex(
                name: "IX_workout_schedules_UserId_is_latest",
                table: "workout_schedules",
                columns: new[] { "UserId", "is_latest" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "schedule_edit_history");

            migrationBuilder.DropTable(
                name: "workout_feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_workout_schedules_parent_schedule_id",
                table: "workout_schedules");

            migrationBuilder.DropIndex(
                name: "IX_workout_schedules_UserId_is_latest",
                table: "workout_schedules");

            migrationBuilder.DropColumn(
                name: "is_latest",
                table: "workout_schedules");

            migrationBuilder.DropColumn(
                name: "parent_schedule_id",
                table: "workout_schedules");

            migrationBuilder.DropColumn(
                name: "regenerated_from_day",
                table: "workout_schedules");

            migrationBuilder.DropColumn(
                name: "regeneration_reason",
                table: "workout_schedules");

            migrationBuilder.DropColumn(
                name: "version",
                table: "workout_schedules");
        }
    }
}

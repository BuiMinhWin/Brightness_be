using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleHistoryTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "schedule_history",
                schema: "personalcoach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserNotes = table.Column<string>(type: "text", nullable: true),
                    FromDayNumber = table.Column<int>(type: "integer", nullable: true),
                    PreviousDifficulty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NewDifficulty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalDays = table.Column<int>(type: "integer", nullable: false),
                    CompletedDays = table.Column<int>(type: "integer", nullable: false),
                    CompletionRate = table.Column<decimal>(type: "numeric", nullable: false),
                    TokensUsed = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    ConfigurationSnapshot = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_schedule_history_workout_schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "personalcoach",
                        principalTable: "workout_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_schedule_history_ActionType",
                schema: "personalcoach",
                table: "schedule_history",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_history_CreatedAt",
                schema: "personalcoach",
                table: "schedule_history",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_history_ScheduleId",
                schema: "personalcoach",
                table: "schedule_history",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_history_UserId",
                schema: "personalcoach",
                table: "schedule_history",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_history_UserId_Version",
                schema: "personalcoach",
                table: "schedule_history",
                columns: new[] { "UserId", "Version" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "schedule_history",
                schema: "personalcoach");
        }
    }
}

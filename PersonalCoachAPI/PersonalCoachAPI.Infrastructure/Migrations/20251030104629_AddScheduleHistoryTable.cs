using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_schedule_history_workout_schedules_ScheduleId",
                schema: "personalcoach",
                table: "schedule_history");

            migrationBuilder.DropPrimaryKey(
                name: "PK_schedule_history",
                schema: "personalcoach",
                table: "schedule_history");

            migrationBuilder.DropIndex(
                name: "IX_schedule_history_UserId_Version",
                schema: "personalcoach",
                table: "schedule_history");

            migrationBuilder.RenameTable(
                name: "schedule_history",
                schema: "personalcoach",
                newName: "schedule_histories",
                newSchema: "personalcoach");

            migrationBuilder.RenameIndex(
                name: "IX_schedule_history_UserId",
                schema: "personalcoach",
                table: "schedule_histories",
                newName: "IX_schedule_histories_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_schedule_history_ScheduleId",
                schema: "personalcoach",
                table: "schedule_histories",
                newName: "IX_schedule_histories_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_schedule_history_CreatedAt",
                schema: "personalcoach",
                table: "schedule_histories",
                newName: "IX_schedule_histories_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_schedule_history_ActionType",
                schema: "personalcoach",
                table: "schedule_histories",
                newName: "IX_schedule_histories_ActionType");

            migrationBuilder.AlterColumn<string>(
                name: "ChangeReason",
                schema: "personalcoach",
                table: "schedule_histories",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_schedule_histories",
                schema: "personalcoach",
                table: "schedule_histories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "schedule_snapshots",
                schema: "personalcoach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScheduleStateSnapshot = table.Column<string>(type: "jsonb", nullable: false),
                    ChangeDetails = table.Column<string>(type: "jsonb", nullable: true),
                    ChangeReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserNote = table.Column<string>(type: "text", nullable: true),
                    TokensUsed = table.Column<int>(type: "integer", nullable: false),
                    AffectedEntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AffectedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    SnapshotNumber = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_schedule_snapshots_workout_schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "personalcoach",
                        principalTable: "workout_schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_schedule_histories_ScheduleId_Version",
                schema: "personalcoach",
                table: "schedule_histories",
                columns: new[] { "ScheduleId", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_schedule_snapshots_ActionType",
                schema: "personalcoach",
                table: "schedule_snapshots",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_snapshots_AffectedEntityType",
                schema: "personalcoach",
                table: "schedule_snapshots",
                column: "AffectedEntityType");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_snapshots_CreatedAt",
                schema: "personalcoach",
                table: "schedule_snapshots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_snapshots_ScheduleId",
                schema: "personalcoach",
                table: "schedule_snapshots",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_snapshots_ScheduleId_SnapshotNumber",
                schema: "personalcoach",
                table: "schedule_snapshots",
                columns: new[] { "ScheduleId", "SnapshotNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_schedule_snapshots_Source",
                schema: "personalcoach",
                table: "schedule_snapshots",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_snapshots_UserId",
                schema: "personalcoach",
                table: "schedule_snapshots",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "schedule_snapshots",
                schema: "personalcoach");

            migrationBuilder.DropPrimaryKey(
                name: "PK_schedule_histories",
                schema: "personalcoach",
                table: "schedule_histories");

            migrationBuilder.DropIndex(
                name: "IX_schedule_histories_ScheduleId_Version",
                schema: "personalcoach",
                table: "schedule_histories");

            migrationBuilder.RenameTable(
                name: "schedule_histories",
                schema: "personalcoach",
                newName: "schedule_history",
                newSchema: "personalcoach");

            migrationBuilder.RenameIndex(
                name: "IX_schedule_histories_UserId",
                schema: "personalcoach",
                table: "schedule_history",
                newName: "IX_schedule_history_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_schedule_histories_ScheduleId",
                schema: "personalcoach",
                table: "schedule_history",
                newName: "IX_schedule_history_ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_schedule_histories_CreatedAt",
                schema: "personalcoach",
                table: "schedule_history",
                newName: "IX_schedule_history_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_schedule_histories_ActionType",
                schema: "personalcoach",
                table: "schedule_history",
                newName: "IX_schedule_history_ActionType");

            migrationBuilder.AlterColumn<string>(
                name: "ChangeReason",
                schema: "personalcoach",
                table: "schedule_history",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_schedule_history",
                schema: "personalcoach",
                table: "schedule_history",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_history_UserId_Version",
                schema: "personalcoach",
                table: "schedule_history",
                columns: new[] { "UserId", "Version" });

            migrationBuilder.AddForeignKey(
                name: "FK_schedule_history_workout_schedules_ScheduleId",
                schema: "personalcoach",
                table: "schedule_history",
                column: "ScheduleId",
                principalSchema: "personalcoach",
                principalTable: "workout_schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

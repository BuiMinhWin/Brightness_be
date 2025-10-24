using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokenLinkReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "broken_link_reports",
                schema: "personalcoach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BrokenVideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReportedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewVideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broken_link_reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_broken_link_reports_ExerciseId",
                schema: "personalcoach",
                table: "broken_link_reports",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_broken_link_reports_ReportedAt",
                schema: "personalcoach",
                table: "broken_link_reports",
                column: "ReportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_broken_link_reports_Status",
                schema: "personalcoach",
                table: "broken_link_reports",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "broken_link_reports",
                schema: "personalcoach");
        }
    }
}

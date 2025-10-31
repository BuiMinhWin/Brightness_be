using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokenLinkReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "broken_link_reports",
                schema: "personalcoach",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exercise_id = table.Column<string>(type: "text", nullable: false),
                    exercise_name = table.Column<string>(type: "text", nullable: false),
                    broken_video_url = table.Column<string>(type: "text", nullable: false),
                    reported_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reported_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "pending"),
                    user_comment = table.Column<string>(type: "text", nullable: true),
                    admin_notes = table.Column<string>(type: "text", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    new_video_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_broken_link_reports", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_broken_link_reports_exercise_id",
                schema: "personalcoach",
                table: "broken_link_reports",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_broken_link_reports_reported_at",
                schema: "personalcoach",
                table: "broken_link_reports",
                column: "reported_at");

            migrationBuilder.CreateIndex(
                name: "IX_broken_link_reports_status",
                schema: "personalcoach",
                table: "broken_link_reports",
                column: "status");
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

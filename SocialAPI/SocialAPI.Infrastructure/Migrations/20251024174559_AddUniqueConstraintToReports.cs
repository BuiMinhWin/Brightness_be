using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, remove duplicate reports, keeping only the oldest one for each user-target combination
            migrationBuilder.Sql(@"
                DELETE FROM social.reports
                WHERE ""Id"" IN (
                    SELECT r.""Id""
                    FROM social.reports r
                    WHERE EXISTS (
                        SELECT 1
                        FROM social.reports r2
                        WHERE r2.""ReporterId"" = r.""ReporterId""
                          AND r2.""TargetId"" = r.""TargetId""
                          AND r2.""TargetType"" = r.""TargetType""
                          AND r2.""Status"" = r.""Status""
                          AND r2.""CreatedAt"" < r.""CreatedAt""
                    )
                );
            ");

            // Now create the unique constraint
            migrationBuilder.CreateIndex(
                name: "IX_Reports_Unique_UserTargetStatus",
                schema: "social",
                table: "reports",
                columns: new[] { "ReporterId", "TargetId", "TargetType", "Status" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_Unique_UserTargetStatus",
                schema: "social",
                table: "reports");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatService.Infrastructure.ChatService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiUsageLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_usage_logs",
                schema: "chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Operation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PromptTokens = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompletionTokens = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalTokens = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_usage_logs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_usage_logs_CreatedAt",
                schema: "chat",
                table: "ai_usage_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ai_usage_logs_SessionId",
                schema: "chat",
                table: "ai_usage_logs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_usage_logs_UserId",
                schema: "chat",
                table: "ai_usage_logs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_usage_logs",
                schema: "chat");
        }
    }
}

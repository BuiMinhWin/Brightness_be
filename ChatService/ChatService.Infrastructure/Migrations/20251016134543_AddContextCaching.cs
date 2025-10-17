using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContextCaching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContextUpdatedAt",
                schema: "chat",
                table: "chat_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserContextSnapshot",
                schema: "chat",
                table: "chat_sessions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_chat_sessions_UserId_ContextUpdatedAt",
                schema: "chat",
                table: "chat_sessions",
                columns: new[] { "UserId", "ContextUpdatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_chat_sessions_UserId_ContextUpdatedAt",
                schema: "chat",
                table: "chat_sessions");

            migrationBuilder.DropColumn(
                name: "ContextUpdatedAt",
                schema: "chat",
                table: "chat_sessions");

            migrationBuilder.DropColumn(
                name: "UserContextSnapshot",
                schema: "chat",
                table: "chat_sessions");
        }
    }
}

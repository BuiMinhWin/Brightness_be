using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChallengeAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetChallengeSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "challenge");

            migrationBuilder.RenameTable(
                name: "challenges",
                newName: "challenges",
                newSchema: "challenge");

            migrationBuilder.RenameTable(
                name: "challenge_tasks",
                newName: "challenge_tasks",
                newSchema: "challenge");

            migrationBuilder.RenameTable(
                name: "challenge_task_progress",
                newName: "challenge_task_progress",
                newSchema: "challenge");

            migrationBuilder.RenameTable(
                name: "challenge_rules",
                newName: "challenge_rules",
                newSchema: "challenge");

            migrationBuilder.RenameTable(
                name: "challenge_participants",
                newName: "challenge_participants",
                newSchema: "challenge");

            migrationBuilder.RenameTable(
                name: "achievements",
                newName: "achievements",
                newSchema: "challenge");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "challenges",
                schema: "challenge",
                newName: "challenges");

            migrationBuilder.RenameTable(
                name: "challenge_tasks",
                schema: "challenge",
                newName: "challenge_tasks");

            migrationBuilder.RenameTable(
                name: "challenge_task_progress",
                schema: "challenge",
                newName: "challenge_task_progress");

            migrationBuilder.RenameTable(
                name: "challenge_rules",
                schema: "challenge",
                newName: "challenge_rules");

            migrationBuilder.RenameTable(
                name: "challenge_participants",
                schema: "challenge",
                newName: "challenge_participants");

            migrationBuilder.RenameTable(
                name: "achievements",
                schema: "challenge",
                newName: "achievements");
        }
    }
}

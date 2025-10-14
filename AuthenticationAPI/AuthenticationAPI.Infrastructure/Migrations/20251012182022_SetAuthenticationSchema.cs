using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetAuthenticationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "authentication");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "users",
                newSchema: "authentication");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                newName: "refresh_tokens",
                newSchema: "authentication");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "users",
                schema: "authentication",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "refresh_tokens",
                schema: "authentication",
                newName: "refresh_tokens");
        }
    }
}

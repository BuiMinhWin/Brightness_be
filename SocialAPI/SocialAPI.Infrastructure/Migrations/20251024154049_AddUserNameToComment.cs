using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNameToComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                schema: "social",
                table: "comments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                schema: "social",
                table: "comments");
        }
    }
}

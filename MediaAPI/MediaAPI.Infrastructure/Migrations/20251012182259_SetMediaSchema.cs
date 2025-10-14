using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetMediaSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "media");

            migrationBuilder.RenameTable(
                name: "user_views",
                newName: "user_views",
                newSchema: "media");

            migrationBuilder.RenameTable(
                name: "user_likes",
                newName: "user_likes",
                newSchema: "media");

            migrationBuilder.RenameTable(
                name: "media_items",
                newName: "media_items",
                newSchema: "media");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "user_views",
                schema: "media",
                newName: "user_views");

            migrationBuilder.RenameTable(
                name: "user_likes",
                schema: "media",
                newName: "user_likes");

            migrationBuilder.RenameTable(
                name: "media_items",
                schema: "media",
                newName: "media_items");
        }
    }
}

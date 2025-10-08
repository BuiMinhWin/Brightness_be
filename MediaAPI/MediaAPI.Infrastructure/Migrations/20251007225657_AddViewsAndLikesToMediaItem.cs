using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddViewsAndLikesToMediaItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Likes",
                table: "media_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "media_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Likes",
                table: "media_items");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "media_items");
        }
    }
}

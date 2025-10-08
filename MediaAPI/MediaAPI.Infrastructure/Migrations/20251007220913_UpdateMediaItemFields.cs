using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaItemFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "media_items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "media_items",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "media_items",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Thumbnail",
                table: "media_items",
                type: "text",
                nullable: true);

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
                name: "Category",
                table: "media_items");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "media_items");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "media_items");

            migrationBuilder.DropColumn(
                name: "Thumbnail",
                table: "media_items");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "media_items");
        }
    }
}

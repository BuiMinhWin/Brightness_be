using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLikeAndView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "media_items");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "media_items");

            migrationBuilder.CreateTable(
                name: "user_likes",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_likes", x => new { x.UserId, x.MediaItemId });
                    table.ForeignKey(
                        name: "FK_user_likes_media_items_MediaItemId",
                        column: x => x.MediaItemId,
                        principalTable: "media_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_views",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaItemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_views", x => new { x.UserId, x.MediaItemId });
                    table.ForeignKey(
                        name: "FK_user_views_media_items_MediaItemId",
                        column: x => x.MediaItemId,
                        principalTable: "media_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_likes_MediaItemId",
                table: "user_likes",
                column: "MediaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_user_views_MediaItemId",
                table: "user_views",
                column: "MediaItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_likes");

            migrationBuilder.DropTable(
                name: "user_views");

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "media_items",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "media_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

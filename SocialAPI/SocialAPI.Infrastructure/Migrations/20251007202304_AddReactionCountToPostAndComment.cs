using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReactionCountToPostAndComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReactionCount",
                table: "posts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReactionCount",
                table: "comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReactionCount",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "ReactionCount",
                table: "comments");
        }
    }
}

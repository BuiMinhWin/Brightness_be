using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetSocialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "social");

            migrationBuilder.RenameTable(
                name: "tags",
                newName: "tags",
                newSchema: "social");

            migrationBuilder.RenameTable(
                name: "reports",
                newName: "reports",
                newSchema: "social");

            migrationBuilder.RenameTable(
                name: "posts",
                newName: "posts",
                newSchema: "social");

            migrationBuilder.RenameTable(
                name: "post_votes",
                newName: "post_votes",
                newSchema: "social");

            migrationBuilder.RenameTable(
                name: "post_tags",
                newName: "post_tags",
                newSchema: "social");

            migrationBuilder.RenameTable(
                name: "post_images",
                newName: "post_images",
                newSchema: "social");

            migrationBuilder.RenameTable(
                name: "comments",
                newName: "comments",
                newSchema: "social");

            migrationBuilder.RenameTable(
                name: "comment_votes",
                newName: "comment_votes",
                newSchema: "social");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "tags",
                schema: "social",
                newName: "tags");

            migrationBuilder.RenameTable(
                name: "reports",
                schema: "social",
                newName: "reports");

            migrationBuilder.RenameTable(
                name: "posts",
                schema: "social",
                newName: "posts");

            migrationBuilder.RenameTable(
                name: "post_votes",
                schema: "social",
                newName: "post_votes");

            migrationBuilder.RenameTable(
                name: "post_tags",
                schema: "social",
                newName: "post_tags");

            migrationBuilder.RenameTable(
                name: "post_images",
                schema: "social",
                newName: "post_images");

            migrationBuilder.RenameTable(
                name: "comments",
                schema: "social",
                newName: "comments");

            migrationBuilder.RenameTable(
                name: "comment_votes",
                schema: "social",
                newName: "comment_votes");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "social",
                table: "posts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "social",
                table: "posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "social",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "social",
                table: "posts");
        }
    }
}

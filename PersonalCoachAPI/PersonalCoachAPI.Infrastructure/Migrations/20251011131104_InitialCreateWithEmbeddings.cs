using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualWeightKg",
                table: "workout_schedule_exercises");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "workout_schedule_exercises");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualWeightKg",
                table: "workout_schedule_exercises",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "workout_schedule_exercises",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}

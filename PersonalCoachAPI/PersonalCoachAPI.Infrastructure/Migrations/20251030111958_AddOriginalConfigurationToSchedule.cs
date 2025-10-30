using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalConfigurationToSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "original_additional_notes",
                schema: "personalcoach",
                table: "workout_schedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "original_available_days",
                schema: "personalcoach",
                table: "workout_schedules",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "original_equipment_available",
                schema: "personalcoach",
                table: "workout_schedules",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "original_additional_notes",
                schema: "personalcoach",
                table: "workout_schedules");

            migrationBuilder.DropColumn(
                name: "original_available_days",
                schema: "personalcoach",
                table: "workout_schedules");

            migrationBuilder.DropColumn(
                name: "original_equipment_available",
                schema: "personalcoach",
                table: "workout_schedules");
        }
    }
}

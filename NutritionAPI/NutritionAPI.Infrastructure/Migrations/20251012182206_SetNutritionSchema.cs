using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutritionAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetNutritionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "nutrition");

            migrationBuilder.RenameTable(
                name: "nutrition_plans",
                newName: "nutrition_plans",
                newSchema: "nutrition");

            migrationBuilder.RenameTable(
                name: "meal_logs",
                newName: "meal_logs",
                newSchema: "nutrition");

            migrationBuilder.RenameTable(
                name: "meal_foods",
                newName: "meal_foods",
                newSchema: "nutrition");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "nutrition_plans",
                schema: "nutrition",
                newName: "nutrition_plans");

            migrationBuilder.RenameTable(
                name: "meal_logs",
                schema: "nutrition",
                newName: "meal_logs");

            migrationBuilder.RenameTable(
                name: "meal_foods",
                schema: "nutrition",
                newName: "meal_foods");
        }
    }
}

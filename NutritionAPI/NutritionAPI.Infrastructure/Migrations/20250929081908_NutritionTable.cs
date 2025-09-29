using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutritionAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NutritionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "nutrition_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CalorieTarget = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    CarbGrams = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    FatGrams = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nutrition_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meal_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    MealType = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    TotalCalories = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    CarbGrams = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    FatGrams = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    NutritionPlanId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_meal_logs_nutrition_plans_NutritionPlanId",
                        column: x => x.NutritionPlanId,
                        principalTable: "nutrition_plans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "meal_foods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MealLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServingUnit = table.Column<string>(type: "text", nullable: true),
                    ServingSize = table.Column<string>(type: "text", nullable: true),
                    Calories = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    ProteinGrams = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    CarbGrams = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    FatGrams = table.Column<decimal>(type: "numeric(6,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_foods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_meal_foods_meal_logs_MealLogId",
                        column: x => x.MealLogId,
                        principalTable: "meal_logs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_meal_foods_MealLogId",
                table: "meal_foods",
                column: "MealLogId");

            migrationBuilder.CreateIndex(
                name: "IX_meal_logs_NutritionPlanId",
                table: "meal_logs",
                column: "NutritionPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meal_foods");

            migrationBuilder.DropTable(
                name: "meal_logs");

            migrationBuilder.DropTable(
                name: "nutrition_plans");
        }
    }
}

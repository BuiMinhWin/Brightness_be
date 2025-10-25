using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditSystemToAIGenerationUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreditCost",
                schema: "personalcoach",
                table: "ai_generation_usage",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OperationType",
                schema: "personalcoach",
                table: "ai_generation_usage",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditCost",
                schema: "personalcoach",
                table: "ai_generation_usage");

            migrationBuilder.DropColumn(
                name: "OperationType",
                schema: "personalcoach",
                table: "ai_generation_usage");
        }
    }
}

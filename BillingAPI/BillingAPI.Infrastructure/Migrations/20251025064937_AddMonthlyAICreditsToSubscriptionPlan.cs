using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyAICreditsToSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MonthlyAICredits",
                schema: "billing",
                table: "subscription_plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyAICredits",
                schema: "billing",
                table: "subscription_plans");
        }
    }
}

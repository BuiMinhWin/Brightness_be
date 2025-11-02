using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditTierToSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreditTier",
                schema: "billing",
                table: "subscription_plans",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "free");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditTier",
                schema: "billing",
                table: "subscription_plans");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetBillingSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.RenameTable(
                name: "subscription_plans",
                newName: "subscription_plans",
                newSchema: "billing");

            migrationBuilder.RenameTable(
                name: "payments",
                newName: "payments",
                newSchema: "billing");

            migrationBuilder.RenameTable(
                name: "invoices",
                newName: "invoices",
                newSchema: "billing");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "subscription_plans",
                schema: "billing",
                newName: "subscription_plans");

            migrationBuilder.RenameTable(
                name: "payments",
                schema: "billing",
                newName: "payments");

            migrationBuilder.RenameTable(
                name: "invoices",
                schema: "billing",
                newName: "invoices");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicesAndPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "payments",
                newName: "InvoiceId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "subscription_plans",
                type: "numeric(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "subscription_plans",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationDays",
                table: "subscription_plans",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "subscription_plans",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "payments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "payments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "payments",
                type: "numeric(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "payments",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiredAt",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gateway",
                table: "payments",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawGatewayPayload",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserExternalId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubscriptionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc' + interval '3 days'"),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoices_subscription_plans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalTable: "subscription_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_plans_Name",
                table: "subscription_plans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_InvoiceId",
                table: "payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_InvoiceCode",
                table: "invoices",
                column: "InvoiceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_SubscriptionPlanId",
                table: "invoices",
                column: "SubscriptionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_invoices_InvoiceId",
                table: "payments",
                column: "InvoiceId",
                principalTable: "invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_invoices_InvoiceId",
                table: "payments");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_subscription_plans_Name",
                table: "subscription_plans");

            migrationBuilder.DropIndex(
                name: "IX_payments_InvoiceId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "DurationDays",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ExpiredAt",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "Gateway",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "RawGatewayPayload",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "payments",
                newName: "SubscriptionId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "subscription_plans",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionId",
                table: "payments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                table: "payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "payments",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,2)");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}

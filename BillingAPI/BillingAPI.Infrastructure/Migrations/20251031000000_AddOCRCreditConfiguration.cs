using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOCRCreditConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add OCR service credit cost configuration
            migrationBuilder.InsertData(
                schema: "billing",
                table: "credit_cost_configurations",
                columns: new[] { "Id", "ServiceName", "OperationType", "BaseTokenMultiplier", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { Guid.NewGuid(), "ocr", "ocr_vision", 2.0m, true, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "billing",
                table: "credit_cost_configurations",
                keyColumn: "ServiceName",
                keyValue: "ocr");
        }
    }
}


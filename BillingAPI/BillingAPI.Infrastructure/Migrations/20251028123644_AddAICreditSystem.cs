using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAICreditSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_credit_pools",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SubscriptionTier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MonthlyTokenLimit = table.Column<int>(type: "integer", nullable: false),
                    RemainingTokens = table.Column<int>(type: "integer", nullable: false),
                    CycleStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CycleEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_credit_pools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "credit_cost_configurations",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OperationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseTokenMultiplier = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_cost_configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ai_operation_logs",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreditPoolId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OperationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PromptTokens = table.Column<int>(type: "integer", nullable: false),
                    CompletionTokens = table.Column<int>(type: "integer", nullable: false),
                    TotalTokens = table.Column<int>(type: "integer", nullable: false),
                    TokenCost = table.Column<int>(type: "integer", nullable: false),
                    EstimatedMonetaryCost = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    RelatedEntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_operation_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ai_operation_logs_ai_credit_pools_CreditPoolId",
                        column: x => x.CreditPoolId,
                        principalSchema: "billing",
                        principalTable: "ai_credit_pools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_credit_pools_UserId_ServiceName",
                schema: "billing",
                table: "ai_credit_pools",
                columns: new[] { "UserId", "ServiceName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ai_operation_logs_CreatedAt",
                schema: "billing",
                table: "ai_operation_logs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ai_operation_logs_CreditPoolId",
                schema: "billing",
                table: "ai_operation_logs",
                column: "CreditPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ai_operation_logs_UserId",
                schema: "billing",
                table: "ai_operation_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_credit_cost_configurations_ServiceName_OperationType",
                schema: "billing",
                table: "credit_cost_configurations",
                columns: new[] { "ServiceName", "OperationType" },
                unique: true);

            // Seed credit cost configurations
            migrationBuilder.InsertData(
                schema: "billing",
                table: "credit_cost_configurations",
                columns: new[] { "Id", "ServiceName", "OperationType", "BaseTokenMultiplier", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { Guid.NewGuid(), "chat", "chat_message", 1.0m, true, DateTime.UtcNow },
                    { Guid.NewGuid(), "schedule", "schedule_generation", 5.0m, true, DateTime.UtcNow },
                    { Guid.NewGuid(), "schedule", "schedule_modification", 3.0m, true, DateTime.UtcNow },
                    { Guid.NewGuid(), "schedule", "schedule_adjustment", 2.0m, true, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete seed data first
            migrationBuilder.DeleteData(
                schema: "billing",
                table: "credit_cost_configurations",
                keyColumn: "ServiceName",
                keyValues: new object[] { "chat", "schedule" });

            migrationBuilder.DropTable(
                name: "ai_operation_logs",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "credit_cost_configurations",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "ai_credit_pools",
                schema: "billing");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIOCRService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ocr_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImageBase64 = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ocr_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ocr_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OCRRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExtractedText = table.Column<string>(type: "text", nullable: false),
                    StructuredData = table.Column<string>(type: "text", nullable: false),
                    Confidence = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ocr_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ocr_results_ocr_requests_OCRRequestId",
                        column: x => x.OCRRequestId,
                        principalTable: "ocr_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "processed_data",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OCRResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProcessedContent = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Insights = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_data", x => x.Id);
                    table.ForeignKey(
                        name: "FK_processed_data_ocr_results_OCRResultId",
                        column: x => x.OCRResultId,
                        principalTable: "ocr_results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ocr_requests_Status",
                table: "ocr_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ocr_requests_UserId",
                table: "ocr_requests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ocr_results_OCRRequestId",
                table: "ocr_results",
                column: "OCRRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_processed_data_DataType",
                table: "processed_data",
                column: "DataType");

            migrationBuilder.CreateIndex(
                name: "IX_processed_data_OCRResultId",
                table: "processed_data",
                column: "OCRResultId");

            migrationBuilder.CreateIndex(
                name: "IX_processed_data_UserId",
                table: "processed_data",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_data");

            migrationBuilder.DropTable(
                name: "ocr_results");

            migrationBuilder.DropTable(
                name: "ocr_requests");
        }
    }
}

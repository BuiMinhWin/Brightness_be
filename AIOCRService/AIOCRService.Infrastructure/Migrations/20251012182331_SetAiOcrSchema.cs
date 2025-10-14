using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIOCRService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetAiOcrSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "aiocr");

            migrationBuilder.RenameTable(
                name: "processed_data",
                newName: "processed_data",
                newSchema: "aiocr");

            migrationBuilder.RenameTable(
                name: "ocr_results",
                newName: "ocr_results",
                newSchema: "aiocr");

            migrationBuilder.RenameTable(
                name: "ocr_requests",
                newName: "ocr_requests",
                newSchema: "aiocr");

            migrationBuilder.AlterColumn<Guid>(
                name: "OCRResultId",
                schema: "aiocr",
                table: "processed_data",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "is_latest",
                schema: "aiocr",
                table: "processed_data",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_processed_data_is_latest",
                schema: "aiocr",
                table: "processed_data",
                column: "is_latest");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_processed_data_is_latest",
                schema: "aiocr",
                table: "processed_data");

            migrationBuilder.DropColumn(
                name: "is_latest",
                schema: "aiocr",
                table: "processed_data");

            migrationBuilder.RenameTable(
                name: "processed_data",
                schema: "aiocr",
                newName: "processed_data");

            migrationBuilder.RenameTable(
                name: "ocr_results",
                schema: "aiocr",
                newName: "ocr_results");

            migrationBuilder.RenameTable(
                name: "ocr_requests",
                schema: "aiocr",
                newName: "ocr_requests");

            migrationBuilder.AlterColumn<Guid>(
                name: "OCRResultId",
                table: "processed_data",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}

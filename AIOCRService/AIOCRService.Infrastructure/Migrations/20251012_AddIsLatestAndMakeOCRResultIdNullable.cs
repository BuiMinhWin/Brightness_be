using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIOCRService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLatestAndMakeOCRResultIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make OCRResultId nullable to support direct AI vision processing
            migrationBuilder.AlterColumn<Guid>(
                name: "OCRResultId",
                table: "processed_data",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Add is_latest column for version tracking
            migrationBuilder.AddColumn<bool>(
                name: "is_latest",
                table: "processed_data",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // Create index for is_latest
            migrationBuilder.CreateIndex(
                name: "IX_processed_data_is_latest",
                table: "processed_data",
                column: "is_latest");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_processed_data_is_latest",
                table: "processed_data");

            migrationBuilder.DropColumn(
                name: "is_latest",
                table: "processed_data");

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


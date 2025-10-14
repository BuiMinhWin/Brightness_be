using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeasurementAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetMeasurementSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "measurement");

            migrationBuilder.RenameTable(
                name: "measurement_devices",
                newName: "measurement_devices",
                newSchema: "measurement");

            migrationBuilder.RenameTable(
                name: "body_measurements",
                newName: "body_measurements",
                newSchema: "measurement");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "measurement_devices",
                schema: "measurement",
                newName: "measurement_devices");

            migrationBuilder.RenameTable(
                name: "body_measurements",
                schema: "measurement",
                newName: "body_measurements");
        }
    }
}

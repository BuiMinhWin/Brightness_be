using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeasurementAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicesAndInBody : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasalMetabolicRate",
                table: "body_measurements",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "body_measurements",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceId",
                table: "body_measurements",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawData",
                table: "body_measurements",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SkeletalMuscleMassKg",
                table: "body_measurements",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "body_measurements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SyncedAt",
                table: "body_measurements",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBodyWaterPercent",
                table: "body_measurements",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeasurementDevice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceType = table.Column<string>(type: "text", nullable: false),
                    Manufacturer = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    SerialNumber = table.Column<string>(type: "text", nullable: true),
                    FirmwareVersion = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementDevice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceSyncLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SyncCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    RawPayload = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSyncLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceSyncLog_MeasurementDevice_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "MeasurementDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_body_measurements_DeviceId",
                table: "body_measurements",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSyncLog_DeviceId",
                table: "DeviceSyncLog",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_body_measurements_MeasurementDevice_DeviceId",
                table: "body_measurements",
                column: "DeviceId",
                principalTable: "MeasurementDevice",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_body_measurements_MeasurementDevice_DeviceId",
                table: "body_measurements");

            migrationBuilder.DropTable(
                name: "DeviceSyncLog");

            migrationBuilder.DropTable(
                name: "MeasurementDevice");

            migrationBuilder.DropIndex(
                name: "IX_body_measurements_DeviceId",
                table: "body_measurements");

            migrationBuilder.DropColumn(
                name: "BasalMetabolicRate",
                table: "body_measurements");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "body_measurements");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "body_measurements");

            migrationBuilder.DropColumn(
                name: "RawData",
                table: "body_measurements");

            migrationBuilder.DropColumn(
                name: "SkeletalMuscleMassKg",
                table: "body_measurements");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "body_measurements");

            migrationBuilder.DropColumn(
                name: "SyncedAt",
                table: "body_measurements");

            migrationBuilder.DropColumn(
                name: "TotalBodyWaterPercent",
                table: "body_measurements");
        }
    }
}

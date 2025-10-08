using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeasurementAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MergeDeviceSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_body_measurements_MeasurementDevice_DeviceId",
                table: "body_measurements");

            migrationBuilder.DropTable(
                name: "DeviceSyncLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeasurementDevice",
                table: "MeasurementDevice");

            migrationBuilder.RenameTable(
                name: "MeasurementDevice",
                newName: "measurement_devices");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "body_measurements",
                type: "numeric(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "WaistCm",
                table: "body_measurements",
                type: "numeric(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalBodyWaterPercent",
                table: "body_measurements",
                type: "numeric(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "body_measurements",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "SkeletalMuscleMassKg",
                table: "body_measurements",
                type: "numeric(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION __tmp_text_to_jsonb(input TEXT)
                RETURNS jsonb AS $$
                BEGIN
                    IF input IS NULL OR trim(input) = '' THEN
                        RETURN NULL;
                    END IF;
                    RETURN input::jsonb;
                EXCEPTION WHEN others THEN
                    RETURN to_jsonb(input);
                END;
                $$ LANGUAGE plpgsql;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE body_measurements
                ALTER COLUMN "RawData"
                TYPE jsonb
                USING __tmp_text_to_jsonb("RawData");
            """);

            migrationBuilder.Sql("""
                DROP FUNCTION __tmp_text_to_jsonb(TEXT);
            """);

            migrationBuilder.AlterColumn<decimal>(
                name: "HipCm",
                table: "body_measurements",
                type: "numeric(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HeightCm",
                table: "body_measurements",
                type: "numeric(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "body_measurements",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<decimal>(
                name: "ChestCm",
                table: "body_measurements",
                type: "numeric(6,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "BasalMetabolicRate",
                table: "body_measurements",
                type: "numeric(8,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                table: "measurement_devices",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegisteredAt",
                table: "measurement_devices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "measurement_devices",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Manufacturer",
                table: "measurement_devices",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "measurement_devices",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "FirmwareVersion",
                table: "measurement_devices",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceType",
                table: "measurement_devices",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncCompletedAt",
                table: "measurement_devices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncMessage",
                table: "measurement_devices",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncPayload",
                table: "measurement_devices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncStartedAt",
                table: "measurement_devices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LastSyncSuccess",
                table: "measurement_devices",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_measurement_devices",
                table: "measurement_devices",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_body_measurements_UserId_RecordedAt",
                table: "body_measurements",
                columns: new[] { "UserId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_measurement_devices_UserId_DeviceType",
                table: "measurement_devices",
                columns: new[] { "UserId", "DeviceType" });

            migrationBuilder.CreateIndex(
                name: "IX_measurement_devices_UserId_SerialNumber",
                table: "measurement_devices",
                columns: new[] { "UserId", "SerialNumber" },
                unique: true,
                filter: "\"SerialNumber\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_body_measurements_measurement_devices_DeviceId",
                table: "body_measurements",
                column: "DeviceId",
                principalTable: "measurement_devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("This migration removes legacy device sync logs and cannot be automatically reverted.");
        }
    }
}


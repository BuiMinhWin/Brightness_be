using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeasurementAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MeasurementTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "body_measurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HeightCm = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    BodyFatPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    WaistCm = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ChestCm = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    HipCm = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ActivityLevel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    WorkoutDaysPerWeek = table.Column<int>(type: "integer", nullable: true),
                    Goal = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HealthNote = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_body_measurements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "body_measurements");
        }
    }
}

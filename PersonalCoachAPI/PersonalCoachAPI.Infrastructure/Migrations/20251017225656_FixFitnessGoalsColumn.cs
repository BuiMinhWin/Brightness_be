using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixFitnessGoalsColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make FitnessGoals column nullable to fix constraint violation
            migrationBuilder.AlterColumn<string>(
                name: "FitnessGoals",
                schema: "personalcoach",
                table: "user_profiles",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: false);

            // Also make ActivityLevel nullable since it's not in the entity model
            migrationBuilder.AlterColumn<string>(
                name: "ActivityLevel",
                schema: "personalcoach",
                table: "user_profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert FitnessGoals column to not null
            migrationBuilder.AlterColumn<string>(
                name: "FitnessGoals",
                schema: "personalcoach",
                table: "user_profiles",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            // Revert ActivityLevel column to not null
            migrationBuilder.AlterColumn<string>(
                name: "ActivityLevel",
                schema: "personalcoach",
                table: "user_profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}

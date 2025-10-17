using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserProfileSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add missing columns to existing user_profiles table
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentWeightKg",
                schema: "personalcoach",
                table: "user_profiles",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FitnessLevel",
                schema: "personalcoach",
                table: "user_profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "beginner");

            migrationBuilder.AddColumn<string>(
                name: "FitnessGoal",
                schema: "personalcoach",
                table: "user_profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Update existing records to have proper default values
            migrationBuilder.Sql(@"
                UPDATE personalcoach.user_profiles 
                SET ""CurrentWeightKg"" = 70.0 
                WHERE ""CurrentWeightKg"" = 0;
            ");

            migrationBuilder.Sql(@"
                UPDATE personalcoach.user_profiles 
                SET ""FitnessGoal"" = 'maintain' 
                WHERE ""FitnessGoal"" = '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the added columns
            migrationBuilder.DropColumn(
                name: "FitnessGoal",
                schema: "personalcoach",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "FitnessLevel",
                schema: "personalcoach",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "CurrentWeightKg",
                schema: "personalcoach",
                table: "user_profiles");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalCoachAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeprecatedWorkoutNutritionSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop deprecated workout and nutrition schemas
            // These were from the old WorkoutAPI and NutritionAPI projects
            // PersonalCoachAPI now handles all workout and nutrition functionality
            
            migrationBuilder.Sql(@"
                DROP SCHEMA IF EXISTS workout CASCADE;
                DROP SCHEMA IF EXISTS nutrition CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot recreate the dropped schemas as we don't have the original structure
            // This migration is intended to be one-way only
            // If rollback is needed, restore from database backup
        }
    }
}


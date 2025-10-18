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
            // Add missing columns to existing user_profiles table using conditional SQL
            // This makes the migration idempotent and prevents "column already exists" errors
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- Add CurrentWeightKg if it doesn't exist
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'personalcoach' 
                        AND table_name = 'user_profiles' 
                        AND column_name = 'CurrentWeightKg'
                    ) THEN
                        ALTER TABLE personalcoach.user_profiles 
                        ADD ""CurrentWeightKg"" numeric NOT NULL DEFAULT 0.0;
                    END IF;

                    -- Add FitnessLevel if it doesn't exist
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'personalcoach' 
                        AND table_name = 'user_profiles' 
                        AND column_name = 'FitnessLevel'
                    ) THEN
                        ALTER TABLE personalcoach.user_profiles 
                        ADD ""FitnessLevel"" character varying(50) NOT NULL DEFAULT 'beginner';
                    END IF;

                    -- Add FitnessGoal if it doesn't exist
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'personalcoach' 
                        AND table_name = 'user_profiles' 
                        AND column_name = 'FitnessGoal'
                    ) THEN
                        ALTER TABLE personalcoach.user_profiles 
                        ADD ""FitnessGoal"" character varying(100) NOT NULL DEFAULT '';
                    END IF;
                END $$;
            ");

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

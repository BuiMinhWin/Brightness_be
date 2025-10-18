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
            // Make columns nullable only if they exist
            // This makes the migration idempotent and prevents "column does not exist" errors
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN
                    -- Make FitnessGoals nullable if it exists
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'personalcoach' 
                        AND table_name = 'user_profiles' 
                        AND column_name = 'FitnessGoals'
                    ) THEN
                        ALTER TABLE personalcoach.user_profiles 
                        ALTER COLUMN ""FitnessGoals"" DROP NOT NULL;
                    END IF;

                    -- Make ActivityLevel nullable if it exists
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_schema = 'personalcoach' 
                        AND table_name = 'user_profiles' 
                        AND column_name = 'ActivityLevel'
                    ) THEN
                        ALTER TABLE personalcoach.user_profiles 
                        ALTER COLUMN ""ActivityLevel"" DROP NOT NULL;
                    END IF;
                END $$;
            ");
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

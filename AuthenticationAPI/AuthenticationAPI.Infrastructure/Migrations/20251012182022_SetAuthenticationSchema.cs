using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetAuthenticationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "authentication");

            migrationBuilder.Sql(
                @"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND table_name = 'users'
    ) THEN
        EXECUTE 'ALTER TABLE public.users SET SCHEMA authentication';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND table_name = 'refresh_tokens'
    ) THEN
        EXECUTE 'ALTER TABLE public.refresh_tokens SET SCHEMA authentication';
    END IF;
END
$$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'authentication'
          AND table_name = 'users'
    ) THEN
        EXECUTE 'ALTER TABLE authentication.users SET SCHEMA public';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'authentication'
          AND table_name = 'refresh_tokens'
    ) THEN
        EXECUTE 'ALTER TABLE authentication.refresh_tokens SET SCHEMA public';
    END IF;
END
$$;");
        }
    }
}

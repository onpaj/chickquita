using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chickquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlockDataIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add CHECK constraints to ensure count fields are non-negative on flocks table
            migrationBuilder.Sql(@"
                ALTER TABLE flocks
                ADD CONSTRAINT ck_flocks_current_hens_non_negative
                CHECK (current_hens >= 0);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE flocks
                ADD CONSTRAINT ck_flocks_current_roosters_non_negative
                CHECK (current_roosters >= 0);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE flocks
                ADD CONSTRAINT ck_flocks_current_chicks_non_negative
                CHECK (current_chicks >= 0);
            ");

            // Add CHECK constraints to ensure count fields are non-negative on flock_history table
            migrationBuilder.Sql(@"
                ALTER TABLE flock_history
                ADD CONSTRAINT ck_flock_history_hens_non_negative
                CHECK (hens >= 0);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE flock_history
                ADD CONSTRAINT ck_flock_history_roosters_non_negative
                CHECK (roosters >= 0);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE flock_history
                ADD CONSTRAINT ck_flock_history_chicks_non_negative
                CHECK (chicks >= 0);
            ");

            // Add trigger to enforce history record immutability (all fields except notes)
            // This trigger prevents updates to immutable fields in flock_history
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION enforce_flock_history_immutability()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Allow updates only to the notes field and updated_at timestamp
                    IF (OLD.tenant_id IS DISTINCT FROM NEW.tenant_id OR
                        OLD.flock_id IS DISTINCT FROM NEW.flock_id OR
                        OLD.change_date IS DISTINCT FROM NEW.change_date OR
                        OLD.hens IS DISTINCT FROM NEW.hens OR
                        OLD.roosters IS DISTINCT FROM NEW.roosters OR
                        OLD.chicks IS DISTINCT FROM NEW.chicks OR
                        OLD.reason IS DISTINCT FROM NEW.reason) THEN
                        RAISE EXCEPTION 'Flock history records are immutable. Only notes field can be updated.';
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER trigger_enforce_flock_history_immutability
                BEFORE UPDATE ON flock_history
                FOR EACH ROW
                EXECUTE FUNCTION enforce_flock_history_immutability();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop immutability trigger and function
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trigger_enforce_flock_history_immutability ON flock_history;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS enforce_flock_history_immutability();");

            // Drop CHECK constraints on flock_history
            migrationBuilder.Sql("ALTER TABLE flock_history DROP CONSTRAINT IF EXISTS ck_flock_history_chicks_non_negative;");
            migrationBuilder.Sql("ALTER TABLE flock_history DROP CONSTRAINT IF EXISTS ck_flock_history_roosters_non_negative;");
            migrationBuilder.Sql("ALTER TABLE flock_history DROP CONSTRAINT IF EXISTS ck_flock_history_hens_non_negative;");

            // Drop CHECK constraints on flocks
            migrationBuilder.Sql("ALTER TABLE flocks DROP CONSTRAINT IF EXISTS ck_flocks_current_chicks_non_negative;");
            migrationBuilder.Sql("ALTER TABLE flocks DROP CONSTRAINT IF EXISTS ck_flocks_current_roosters_non_negative;");
            migrationBuilder.Sql("ALTER TABLE flocks DROP CONSTRAINT IF EXISTS ck_flocks_current_hens_non_negative;");
        }
    }
}

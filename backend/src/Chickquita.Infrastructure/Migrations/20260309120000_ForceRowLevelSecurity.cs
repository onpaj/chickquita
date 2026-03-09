using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chickquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForceRowLevelSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FORCE ROW LEVEL SECURITY makes RLS policies apply even to table owners
            // and superusers. Without this, PostgreSQL bypasses RLS for the role that
            // owns the tables, which is the role used by the application in most setups.
            migrationBuilder.Sql("ALTER TABLE tenants FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE coops FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE flocks FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE flock_history FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE daily_records FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE purchases FORCE ROW LEVEL SECURITY;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE tenants NO FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE coops NO FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE flocks NO FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE flock_history NO FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE daily_records NO FORCE ROW LEVEL SECURITY;");
            migrationBuilder.Sql("ALTER TABLE purchases NO FORCE ROW LEVEL SECURITY;");
        }
    }
}

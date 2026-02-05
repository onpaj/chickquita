using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChickenTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowLevelSecurityToTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create set_tenant_context() function
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION set_tenant_context(tenant_uuid UUID)
                RETURNS void AS $$
                BEGIN
                    PERFORM set_config('app.current_tenant_id', tenant_uuid::text, false);
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Enable Row-Level Security on tenants table
            migrationBuilder.Sql("ALTER TABLE tenants ENABLE ROW LEVEL SECURITY;");

            // Create tenant_isolation policy
            // This policy ensures that each tenant can only see their own record
            migrationBuilder.Sql(@"
                CREATE POLICY tenant_isolation ON tenants
                    USING (id = current_setting('app.current_tenant_id', true)::UUID);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tenant_isolation policy
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON tenants;");

            // Disable Row-Level Security on tenants table
            migrationBuilder.Sql("ALTER TABLE tenants DISABLE ROW LEVEL SECURITY;");

            // Drop set_tenant_context() function
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS set_tenant_context(UUID);");
        }
    }
}

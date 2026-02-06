using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chickquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceCoopsTableSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add is_active column
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "coops",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "ix_coops_created_at",
                table: "coops",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_coops_is_active",
                table: "coops",
                column: "is_active");

            // Create unique constraint on (tenant_id, LOWER(name)) where is_active = true
            // This ensures that a tenant cannot have duplicate active coop names (case-insensitive)
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ix_coops_tenant_id_name_unique_active
                ON coops (tenant_id, LOWER(name))
                WHERE is_active = true;
            ");

            // Enable Row-Level Security on coops table
            migrationBuilder.Sql("ALTER TABLE coops ENABLE ROW LEVEL SECURITY;");

            // Create tenant_isolation policy for coops
            // This policy ensures that each tenant can only access their own coops
            migrationBuilder.Sql(@"
                CREATE POLICY tenant_isolation ON coops
                    USING (tenant_id = current_setting('app.current_tenant_id', true)::UUID);
            ");

            // Create trigger function for auto-updating updated_at
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_updated_at_column()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.updated_at = NOW();
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger on coops table
            migrationBuilder.Sql(@"
                CREATE TRIGGER update_coops_updated_at
                BEFORE UPDATE ON coops
                FOR EACH ROW
                EXECUTE FUNCTION update_updated_at_column();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop trigger
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS update_coops_updated_at ON coops;");

            // Drop trigger function
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_updated_at_column();");

            // Drop tenant_isolation policy
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON coops;");

            // Disable Row-Level Security on coops table
            migrationBuilder.Sql("ALTER TABLE coops DISABLE ROW LEVEL SECURITY;");

            // Drop unique constraint
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_coops_tenant_id_name_unique_active;");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "ix_coops_created_at",
                table: "coops");

            migrationBuilder.DropIndex(
                name: "ix_coops_is_active",
                table: "coops");

            // Drop column
            migrationBuilder.DropColumn(
                name: "is_active",
                table: "coops");
        }
    }
}

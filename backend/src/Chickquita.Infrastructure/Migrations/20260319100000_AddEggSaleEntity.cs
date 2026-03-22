using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chickquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEggSaleEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "egg_sales",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price_per_unit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    buyer_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_egg_sales", x => x.id);
                    table.ForeignKey(
                        name: "FK_egg_sales_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_egg_sales_date",
                table: "egg_sales",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_egg_sales_tenant_id",
                table: "egg_sales",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_egg_sales_tenant_id_date",
                table: "egg_sales",
                columns: new[] { "tenant_id", "date" });

            // Enable Row-Level Security on egg_sales table
            migrationBuilder.Sql("ALTER TABLE egg_sales ENABLE ROW LEVEL SECURITY;");

            // Create tenant_isolation policy for egg_sales
            migrationBuilder.Sql(@"
                CREATE POLICY tenant_isolation ON egg_sales
                    USING (tenant_id = current_setting('app.current_tenant_id', true)::UUID);
            ");

            // Create trigger on egg_sales table for auto-updating updated_at
            migrationBuilder.Sql(@"
                CREATE TRIGGER update_egg_sales_updated_at
                BEFORE UPDATE ON egg_sales
                FOR EACH ROW
                EXECUTE FUNCTION update_updated_at_column();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop trigger
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS update_egg_sales_updated_at ON egg_sales;");

            // Drop tenant_isolation policy
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON egg_sales;");

            // Disable Row-Level Security on egg_sales table
            migrationBuilder.Sql("ALTER TABLE egg_sales DISABLE ROW LEVEL SECURITY;");

            // Drop table
            migrationBuilder.DropTable(
                name: "egg_sales");
        }
    }
}

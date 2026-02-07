using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chickquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coop_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    consumed_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchases", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchases_coops_coop_id",
                        column: x => x.coop_id,
                        principalTable: "coops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_purchases_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_purchases_coop_id",
                table: "purchases",
                column: "coop_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchases_purchase_date",
                table: "purchases",
                column: "purchase_date");

            migrationBuilder.CreateIndex(
                name: "ix_purchases_tenant_id",
                table: "purchases",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchases_tenant_id_purchase_date",
                table: "purchases",
                columns: new[] { "tenant_id", "purchase_date" });

            migrationBuilder.CreateIndex(
                name: "ix_purchases_type",
                table: "purchases",
                column: "type");

            // Enable Row-Level Security on purchases table
            migrationBuilder.Sql("ALTER TABLE purchases ENABLE ROW LEVEL SECURITY;");

            // Create tenant_isolation policy for purchases
            // This policy ensures that each tenant can only access their own purchases
            migrationBuilder.Sql(@"
                CREATE POLICY tenant_isolation ON purchases
                    USING (tenant_id = current_setting('app.current_tenant_id', true)::UUID);
            ");

            // Create trigger on purchases table for auto-updating updated_at
            // Note: The trigger function update_updated_at_column() was created in migration EnhanceCoopsTableSchema
            migrationBuilder.Sql(@"
                CREATE TRIGGER update_purchases_updated_at
                BEFORE UPDATE ON purchases
                FOR EACH ROW
                EXECUTE FUNCTION update_updated_at_column();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop trigger
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS update_purchases_updated_at ON purchases;");

            // Drop tenant_isolation policy
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON purchases;");

            // Disable Row-Level Security on purchases table
            migrationBuilder.Sql("ALTER TABLE purchases DISABLE ROW LEVEL SECURITY;");

            // Drop table
            migrationBuilder.DropTable(
                name: "purchases");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chickquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlocksAndFlockHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "flocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    hatch_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    current_hens = table.Column<int>(type: "integer", nullable: false),
                    current_roosters = table.Column<int>(type: "integer", nullable: false),
                    current_chicks = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flocks", x => x.id);
                    table.ForeignKey(
                        name: "FK_flocks_coops_coop_id",
                        column: x => x.coop_id,
                        principalTable: "coops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flock_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    change_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hens = table.Column<int>(type: "integer", nullable: false),
                    roosters = table.Column<int>(type: "integer", nullable: false),
                    chicks = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flock_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_flock_history_flocks_flock_id",
                        column: x => x.flock_id,
                        principalTable: "flocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_flock_history_change_date",
                table: "flock_history",
                column: "change_date");

            migrationBuilder.CreateIndex(
                name: "ix_flock_history_flock_id",
                table: "flock_history",
                column: "flock_id");

            migrationBuilder.CreateIndex(
                name: "ix_flock_history_tenant_id",
                table: "flock_history",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_flocks_coop_id",
                table: "flocks",
                column: "coop_id");

            migrationBuilder.CreateIndex(
                name: "ix_flocks_coop_id_identifier",
                table: "flocks",
                columns: new[] { "coop_id", "identifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_flocks_hatch_date",
                table: "flocks",
                column: "hatch_date");

            migrationBuilder.CreateIndex(
                name: "ix_flocks_is_active",
                table: "flocks",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_flocks_tenant_id",
                table: "flocks",
                column: "tenant_id");

            // Enable Row-Level Security on flocks table
            migrationBuilder.Sql("ALTER TABLE flocks ENABLE ROW LEVEL SECURITY;");

            // Create tenant_isolation policy for flocks
            // This policy ensures that each tenant can only access their own flocks
            migrationBuilder.Sql(@"
                CREATE POLICY tenant_isolation ON flocks
                    USING (tenant_id = current_setting('app.current_tenant_id', true)::UUID);
            ");

            // Enable Row-Level Security on flock_history table
            migrationBuilder.Sql("ALTER TABLE flock_history ENABLE ROW LEVEL SECURITY;");

            // Create tenant_isolation policy for flock_history
            // This policy ensures that each tenant can only access their own flock history entries
            migrationBuilder.Sql(@"
                CREATE POLICY tenant_isolation ON flock_history
                    USING (tenant_id = current_setting('app.current_tenant_id', true)::UUID);
            ");

            // Create trigger function for auto-updating updated_at timestamp on flocks
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_flocks_updated_at()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.updated_at = NOW();
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger for flocks table
            migrationBuilder.Sql(@"
                CREATE TRIGGER trigger_flocks_updated_at
                BEFORE UPDATE ON flocks
                FOR EACH ROW
                EXECUTE FUNCTION update_flocks_updated_at();
            ");

            // Create trigger function for auto-updating updated_at timestamp on flock_history
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_flock_history_updated_at()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.updated_at = NOW();
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger for flock_history table
            migrationBuilder.Sql(@"
                CREATE TRIGGER trigger_flock_history_updated_at
                BEFORE UPDATE ON flock_history
                FOR EACH ROW
                EXECUTE FUNCTION update_flock_history_updated_at();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop triggers
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trigger_flock_history_updated_at ON flock_history;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trigger_flocks_updated_at ON flocks;");

            // Drop trigger functions
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_flock_history_updated_at();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_flocks_updated_at();");

            // Drop RLS policies
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON flock_history;");
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON flocks;");

            migrationBuilder.DropTable(
                name: "flock_history");

            migrationBuilder.DropTable(
                name: "flocks");
        }
    }
}

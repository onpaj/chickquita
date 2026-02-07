using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chickquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyRecordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    record_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    egg_count = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_daily_records_flocks_flock_id",
                        column: x => x.flock_id,
                        principalTable: "flocks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_daily_records_flock_id",
                table: "daily_records",
                column: "flock_id");

            migrationBuilder.CreateIndex(
                name: "ix_daily_records_flock_id_record_date",
                table: "daily_records",
                columns: new[] { "flock_id", "record_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_daily_records_record_date",
                table: "daily_records",
                column: "record_date");

            migrationBuilder.CreateIndex(
                name: "ix_daily_records_tenant_id",
                table: "daily_records",
                column: "tenant_id");

            // Enable Row-Level Security on daily_records table
            migrationBuilder.Sql("ALTER TABLE daily_records ENABLE ROW LEVEL SECURITY;");

            // Create tenant_isolation policy for daily_records
            // This policy ensures that each tenant can only access their own daily records
            migrationBuilder.Sql(@"
                CREATE POLICY tenant_isolation ON daily_records
                    USING (tenant_id = current_setting('app.current_tenant_id', true)::UUID);
            ");

            // Create trigger function for auto-updating updated_at timestamp on daily_records
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_daily_records_updated_at()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.updated_at = NOW();
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger for daily_records table
            migrationBuilder.Sql(@"
                CREATE TRIGGER trigger_daily_records_updated_at
                BEFORE UPDATE ON daily_records
                FOR EACH ROW
                EXECUTE FUNCTION update_daily_records_updated_at();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop trigger
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trigger_daily_records_updated_at ON daily_records;");

            // Drop trigger function
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_daily_records_updated_at();");

            // Drop RLS policy
            migrationBuilder.Sql("DROP POLICY IF EXISTS tenant_isolation ON daily_records;");

            // Disable Row-Level Security on daily_records table
            migrationBuilder.Sql("ALTER TABLE daily_records DISABLE ROW LEVEL SECURITY;");

            migrationBuilder.DropTable(
                name: "daily_records");
        }
    }
}

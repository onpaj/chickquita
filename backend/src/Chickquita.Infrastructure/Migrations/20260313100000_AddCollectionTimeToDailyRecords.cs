using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chickquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionTimeToDailyRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "collection_time",
                table: "daily_records",
                type: "time",
                nullable: true);

            migrationBuilder.DropIndex(
                name: "ix_daily_records_flock_id_record_date",
                table: "daily_records");

            migrationBuilder.CreateIndex(
                name: "ix_daily_records_flock_id_record_date",
                table: "daily_records",
                columns: new[] { "flock_id", "record_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_daily_records_flock_id_record_date",
                table: "daily_records");

            migrationBuilder.CreateIndex(
                name: "ix_daily_records_flock_id_record_date",
                table: "daily_records",
                columns: new[] { "flock_id", "record_date" },
                unique: true);

            migrationBuilder.DropColumn(
                name: "collection_time",
                table: "daily_records");
        }
    }
}

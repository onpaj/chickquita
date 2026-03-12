using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chickquita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameClerkUserIdToClerkOrgId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tenants_email",
                table: "tenants");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "tenants",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "clerk_user_id",
                table: "tenants",
                newName: "clerk_org_id");

            migrationBuilder.RenameIndex(
                name: "ix_tenants_clerk_user_id",
                table: "tenants",
                newName: "ix_tenants_clerk_org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "tenants",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "clerk_org_id",
                table: "tenants",
                newName: "clerk_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_tenants_clerk_org_id",
                table: "tenants",
                newName: "ix_tenants_clerk_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_email",
                table: "tenants",
                column: "email");
        }
    }
}

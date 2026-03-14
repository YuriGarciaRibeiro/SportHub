using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RefactorUserRoleEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tenant_placeholder");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "tenant_placeholder");

            migrationBuilder.RenameTable(
                name: "Sports",
                newName: "Sports",
                newSchema: "tenant_placeholder");

            migrationBuilder.RenameTable(
                name: "Reservations",
                newName: "Reservations",
                newSchema: "tenant_placeholder");

            migrationBuilder.RenameTable(
                name: "CourtSport",
                newName: "CourtSport",
                newSchema: "tenant_placeholder");

            migrationBuilder.RenameTable(
                name: "Courts",
                newName: "Courts",
                newSchema: "tenant_placeholder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Users",
                schema: "tenant_placeholder",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Sports",
                schema: "tenant_placeholder",
                newName: "Sports");

            migrationBuilder.RenameTable(
                name: "Reservations",
                schema: "tenant_placeholder",
                newName: "Reservations");

            migrationBuilder.RenameTable(
                name: "CourtSport",
                schema: "tenant_placeholder",
                newName: "CourtSport");

            migrationBuilder.RenameTable(
                name: "Courts",
                schema: "tenant_placeholder",
                newName: "Courts");
        }
    }
}

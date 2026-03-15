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
            // No-op: RenameTable operations removed.
            // The schema is set dynamically per-tenant via HasDefaultSchema in OnModelCreating.
            // The Role column type (text) is unchanged — no DDL needed.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op: nothing to revert.
        }
    }
}

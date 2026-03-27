using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantEntityBaseClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PeakHoursEnabled",
                schema: "public",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "PeakEndTime",
                table: "Courts",
                type: "time without time zone",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "PeakStartTime",
                table: "Courts",
                type: "time without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeakHoursEnabled",
                schema: "public",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PeakEndTime",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "PeakStartTime",
                table: "Courts");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingFieldsToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPeakHours",
                table: "Reservations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerHour",
                table: "Reservations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PeakPricePerHour",
                table: "Courts",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Courts_Tenants_TenantId",
                table: "Courts",
                column: "TenantId",
                principalSchema: "public",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courts_Tenants_TenantId",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "IsPeakHours",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PeakPricePerHour",
                table: "Courts");
        }
    }
}

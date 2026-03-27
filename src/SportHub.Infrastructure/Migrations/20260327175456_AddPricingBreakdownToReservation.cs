using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingBreakdownToReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NormalPricePerSlot",
                table: "Reservations",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NormalSlots",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NormalSubtotal",
                table: "Reservations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PeakPricePerSlot",
                table: "Reservations",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeakSlots",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PeakSubtotal",
                table: "Reservations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalPricePerSlot",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "NormalSlots",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "NormalSubtotal",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PeakPricePerSlot",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PeakSlots",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PeakSubtotal",
                table: "Reservations");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourtCancellationPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CancelationWindowHours",
                table: "Courts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LateCancellationFeePercent",
                table: "Courts",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelationWindowHours",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "LateCancellationFeePercent",
                table: "Courts");
        }
    }
}

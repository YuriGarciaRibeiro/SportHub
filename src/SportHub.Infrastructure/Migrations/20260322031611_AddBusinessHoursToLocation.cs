using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessHoursToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpeningHours",
                table: "Locations");

            migrationBuilder.AddColumn<string>(
                name: "BusinessHours",
                table: "Locations",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessHours",
                table: "Locations");

            migrationBuilder.AddColumn<string>(
                name: "OpeningHours",
                table: "Locations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}

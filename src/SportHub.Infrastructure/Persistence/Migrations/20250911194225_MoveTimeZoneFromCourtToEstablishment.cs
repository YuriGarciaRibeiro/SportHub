using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveTimeZoneFromCourtToEstablishment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Courts");

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Establishments");

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Courts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

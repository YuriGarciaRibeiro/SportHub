using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetTimeZoneDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "America/Maceio",
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "Establishments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "America/Maceio");
        }
    }
}

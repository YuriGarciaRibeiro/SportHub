using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Establishments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Address_State",
                table: "Establishments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Address_City",
                table: "Establishments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Courts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Sports_Name",
                table: "Sports",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CourtId_StartTimeUtc",
                table: "Reservations",
                columns: new[] { "CourtId", "StartTimeUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentUsers_Role",
                table: "EstablishmentUsers",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentUsers_UserId",
                table: "EstablishmentUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_Address_City",
                table: "Establishments",
                column: "Address_City");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_Address_State",
                table: "Establishments",
                column: "Address_State");

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_Name",
                table: "Establishments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_Name",
                table: "Courts",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Sports_Name",
                table: "Sports");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_CourtId_StartTimeUtc",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_EstablishmentUsers_Role",
                table: "EstablishmentUsers");

            migrationBuilder.DropIndex(
                name: "IX_EstablishmentUsers_UserId",
                table: "EstablishmentUsers");

            migrationBuilder.DropIndex(
                name: "IX_Establishments_Address_City",
                table: "Establishments");

            migrationBuilder.DropIndex(
                name: "IX_Establishments_Address_State",
                table: "Establishments");

            migrationBuilder.DropIndex(
                name: "IX_Establishments_Name",
                table: "Establishments");

            migrationBuilder.DropIndex(
                name: "IX_Courts_Name",
                table: "Courts");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sports",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Establishments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Address_State",
                table: "Establishments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address_City",
                table: "Establishments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Courts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}

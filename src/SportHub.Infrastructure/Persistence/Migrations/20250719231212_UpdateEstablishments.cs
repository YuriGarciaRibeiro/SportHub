using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEstablishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Establishments",
                newName: "Website");

            migrationBuilder.AddColumn<string>(
                name: "Address_City",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Complement",
                table: "Establishments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address_Neighborhood",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Number",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_State",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_Street",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address_ZipCode",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Establishments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Establishments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address_City",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "Address_Complement",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "Address_Neighborhood",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "Address_Number",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "Address_State",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "Address_Street",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "Address_ZipCode",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Establishments");

            migrationBuilder.RenameColumn(
                name: "Website",
                table: "Establishments",
                newName: "Address");
        }
    }
}

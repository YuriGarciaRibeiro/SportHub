using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSportInEstablishment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Establishments_Sports_SportId",
                table: "Establishments");

            migrationBuilder.DropIndex(
                name: "IX_Establishments_SportId",
                table: "Establishments");

            migrationBuilder.DropColumn(
                name: "SportId",
                table: "Establishments");

            migrationBuilder.CreateTable(
                name: "EstablishmentSport",
                columns: table => new
                {
                    EstablishmentsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SportsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstablishmentSport", x => new { x.EstablishmentsId, x.SportsId });
                    table.ForeignKey(
                        name: "FK_EstablishmentSport_Establishments_EstablishmentsId",
                        column: x => x.EstablishmentsId,
                        principalTable: "Establishments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstablishmentSport_Sports_SportsId",
                        column: x => x.SportsId,
                        principalTable: "Sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentSport_SportsId",
                table: "EstablishmentSport",
                column: "SportsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstablishmentSport");

            migrationBuilder.AddColumn<Guid>(
                name: "SportId",
                table: "Establishments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Establishments_SportId",
                table: "Establishments",
                column: "SportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Establishments_Sports_SportId",
                table: "Establishments",
                column: "SportId",
                principalTable: "Sports",
                principalColumn: "Id");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSportInEstablishmentKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstablishmentSport_Establishments_EstablishmentsId",
                table: "EstablishmentSport");

            migrationBuilder.DropForeignKey(
                name: "FK_EstablishmentSport_Sports_SportsId",
                table: "EstablishmentSport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstablishmentSport",
                table: "EstablishmentSport");

            migrationBuilder.RenameTable(
                name: "EstablishmentSport",
                newName: "EstablishmentSports");

            migrationBuilder.RenameColumn(
                name: "SportsId",
                table: "EstablishmentSports",
                newName: "SportId");

            migrationBuilder.RenameColumn(
                name: "EstablishmentsId",
                table: "EstablishmentSports",
                newName: "EstablishmentId");

            migrationBuilder.RenameIndex(
                name: "IX_EstablishmentSport_SportsId",
                table: "EstablishmentSports",
                newName: "IX_EstablishmentSports_SportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstablishmentSports",
                table: "EstablishmentSports",
                columns: new[] { "EstablishmentId", "SportId" });

            migrationBuilder.CreateIndex(
                name: "IX_EstablishmentSports_EstablishmentId_SportId",
                table: "EstablishmentSports",
                columns: new[] { "EstablishmentId", "SportId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EstablishmentSports_Establishments_EstablishmentId",
                table: "EstablishmentSports",
                column: "EstablishmentId",
                principalTable: "Establishments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EstablishmentSports_Sports_SportId",
                table: "EstablishmentSports",
                column: "SportId",
                principalTable: "Sports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EstablishmentSports_Establishments_EstablishmentId",
                table: "EstablishmentSports");

            migrationBuilder.DropForeignKey(
                name: "FK_EstablishmentSports_Sports_SportId",
                table: "EstablishmentSports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EstablishmentSports",
                table: "EstablishmentSports");

            migrationBuilder.DropIndex(
                name: "IX_EstablishmentSports_EstablishmentId_SportId",
                table: "EstablishmentSports");

            migrationBuilder.RenameTable(
                name: "EstablishmentSports",
                newName: "EstablishmentSport");

            migrationBuilder.RenameColumn(
                name: "SportId",
                table: "EstablishmentSport",
                newName: "SportsId");

            migrationBuilder.RenameColumn(
                name: "EstablishmentId",
                table: "EstablishmentSport",
                newName: "EstablishmentsId");

            migrationBuilder.RenameIndex(
                name: "IX_EstablishmentSports_SportId",
                table: "EstablishmentSport",
                newName: "IX_EstablishmentSport_SportsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EstablishmentSport",
                table: "EstablishmentSport",
                columns: new[] { "EstablishmentsId", "SportsId" });

            migrationBuilder.AddForeignKey(
                name: "FK_EstablishmentSport_Establishments_EstablishmentsId",
                table: "EstablishmentSport",
                column: "EstablishmentsId",
                principalTable: "Establishments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EstablishmentSport_Sports_SportsId",
                table: "EstablishmentSport",
                column: "SportsId",
                principalTable: "Sports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSportInCourt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courts_Sports_SportId",
                table: "Courts");

            migrationBuilder.DropIndex(
                name: "IX_Courts_SportId",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "SportId",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "SportType",
                table: "Courts");

            migrationBuilder.CreateTable(
                name: "CourtSport",
                columns: table => new
                {
                    CourtsId = table.Column<Guid>(type: "uuid", nullable: false),
                    SportsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtSport", x => new { x.CourtsId, x.SportsId });
                    table.ForeignKey(
                        name: "FK_CourtSport_Courts_CourtsId",
                        column: x => x.CourtsId,
                        principalTable: "Courts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourtSport_Sports_SportsId",
                        column: x => x.SportsId,
                        principalTable: "Sports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourtSport_SportsId",
                table: "CourtSport",
                column: "SportsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourtSport");

            migrationBuilder.AddColumn<Guid>(
                name: "SportId",
                table: "Courts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SportType",
                table: "Courts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_SportId",
                table: "Courts",
                column: "SportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courts_Sports_SportId",
                table: "Courts",
                column: "SportId",
                principalTable: "Sports",
                principalColumn: "Id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class updateCups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CupMatches_CupId_Round_MatchNumber",
                table: "CupMatches");

            migrationBuilder.AddColumn<int>(
                name: "Draws",
                table: "CupTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GoalsAgainst",
                table: "CupTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GoalsFor",
                table: "CupTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "CupTeams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Losses",
                table: "CupTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "CupTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Wins",
                table: "CupTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GroupSize",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<int>(
                name: "TeamsAdvancingPerGroup",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "CupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "CupMatches",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CupGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CupId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GroupNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CupGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CupGroups_Cups_CupId",
                        column: x => x.CupId,
                        principalTable: "Cups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CupTeams_GroupId",
                table: "CupTeams",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CupMatches_CupId_Round_MatchNumber_Stage",
                table: "CupMatches",
                columns: new[] { "CupId", "Round", "MatchNumber", "Stage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CupMatches_GroupId",
                table: "CupMatches",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CupGroups_CupId_GroupNumber",
                table: "CupGroups",
                columns: new[] { "CupId", "GroupNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CupMatches_CupGroups_GroupId",
                table: "CupMatches",
                column: "GroupId",
                principalTable: "CupGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CupTeams_CupGroups_GroupId",
                table: "CupTeams",
                column: "GroupId",
                principalTable: "CupGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CupMatches_CupGroups_GroupId",
                table: "CupMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_CupTeams_CupGroups_GroupId",
                table: "CupTeams");

            migrationBuilder.DropTable(
                name: "CupGroups");

            migrationBuilder.DropIndex(
                name: "IX_CupTeams_GroupId",
                table: "CupTeams");

            migrationBuilder.DropIndex(
                name: "IX_CupMatches_CupId_Round_MatchNumber_Stage",
                table: "CupMatches");

            migrationBuilder.DropIndex(
                name: "IX_CupMatches_GroupId",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "Draws",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "GoalsAgainst",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "GoalsFor",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "Losses",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "Wins",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "GroupSize",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "TeamsAdvancingPerGroup",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "CupMatches");

            migrationBuilder.CreateIndex(
                name: "IX_CupMatches_CupId_Round_MatchNumber",
                table: "CupMatches",
                columns: new[] { "CupId", "Round", "MatchNumber" },
                unique: true);
        }
    }
}

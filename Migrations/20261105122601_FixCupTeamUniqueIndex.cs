using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixCupTeamUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams");

            migrationBuilder.AlterColumn<int>(
                name: "CaptainUserId",
                table: "CupTeams",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams",
                columns: new[] { "CupId", "CaptainUserId" },
                unique: true,
                filter: "[CaptainUserId] IS NOT NULL AND [Status] <> N'Rejected'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams");

            migrationBuilder.AlterColumn<int>(
                name: "CaptainUserId",
                table: "CupTeams",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams",
                columns: new[] { "CupId", "CaptainUserId" },
                unique: true,
                filter: "[Status] <> N'Rejected'");
        }
    }
}

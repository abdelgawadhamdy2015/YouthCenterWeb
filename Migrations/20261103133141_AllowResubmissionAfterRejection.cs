using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class AllowResubmissionAfterRejection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams");

            migrationBuilder.CreateIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams",
                columns: new[] { "CupId", "CaptainUserId" },
                unique: true,
                filter: "[Status] <> N'Rejected'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams");

            migrationBuilder.CreateIndex(
                name: "IX_CupTeams_CupId_CaptainUserId",
                table: "CupTeams",
                columns: new[] { "CupId", "CaptainUserId" },
                unique: true);
        }
    }
}

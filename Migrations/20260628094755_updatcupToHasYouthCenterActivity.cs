using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class updatcupToHasYouthCenterActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cups_Activities_ActivityId",
                table: "Cups");

            migrationBuilder.DropForeignKey(
                name: "FK_Cups_YouthCenters_YouthCenterId",
                table: "Cups");

            migrationBuilder.DropIndex(
                name: "IX_Cups_ActivityId",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Cups");

            migrationBuilder.RenameColumn(
                name: "YouthCenterId",
                table: "Cups",
                newName: "YouthCenterActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_Cups_YouthCenterId",
                table: "Cups",
                newName: "IX_Cups_YouthCenterActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cups_YouthCenterActivities_YouthCenterActivityId",
                table: "Cups",
                column: "YouthCenterActivityId",
                principalTable: "YouthCenterActivities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cups_YouthCenterActivities_YouthCenterActivityId",
                table: "Cups");

            migrationBuilder.RenameColumn(
                name: "YouthCenterActivityId",
                table: "Cups",
                newName: "YouthCenterId");

            migrationBuilder.RenameIndex(
                name: "IX_Cups_YouthCenterActivityId",
                table: "Cups",
                newName: "IX_Cups_YouthCenterId");

            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cups_ActivityId",
                table: "Cups",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cups_Activities_ActivityId",
                table: "Cups",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cups_YouthCenters_YouthCenterId",
                table: "Cups",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class e7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId1",
                table: "YouthCenterActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_ActivityId",
                table: "YouthCenterActivity");

            migrationBuilder.DropIndex(
                name: "IX_YouthCenterActivity_ActivityId1",
                table: "YouthCenterActivity");

            migrationBuilder.DropColumn(
                name: "ActivityId1",
                table: "YouthCenterActivity");

            migrationBuilder.CreateIndex(
                name: "IX_YouthCenterActivity_YouthCenterId",
                table: "YouthCenterActivity",
                column: "YouthCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId",
                table: "YouthCenterActivity",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_YouthCenterId",
                table: "YouthCenterActivity",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId",
                table: "YouthCenterActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_YouthCenterId",
                table: "YouthCenterActivity");

            migrationBuilder.DropIndex(
                name: "IX_YouthCenterActivity_YouthCenterId",
                table: "YouthCenterActivity");

            migrationBuilder.AddColumn<int>(
                name: "ActivityId1",
                table: "YouthCenterActivity",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_YouthCenterActivity_ActivityId1",
                table: "YouthCenterActivity",
                column: "ActivityId1");

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId1",
                table: "YouthCenterActivity",
                column: "ActivityId1",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_ActivityId",
                table: "YouthCenterActivity",
                column: "ActivityId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

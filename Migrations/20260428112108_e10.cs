using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class e10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_YouthCenterId",
                table: "YouthCenterActivity");

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_YouthCenterId",
                table: "YouthCenterActivity",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_YouthCenterId",
                table: "YouthCenterActivity");

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_YouthCenterId",
                table: "YouthCenterActivity",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

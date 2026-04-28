using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class e12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_YouthCenterActivity_ActivityId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_YouthCenterActivity_YouthCenterActivityId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_YouthCenters_YouthCenterId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId",
                table: "YouthCenterActivity");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ActivityId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ActivityId",
                table: "Reservations");

            migrationBuilder.AlterColumn<int>(
                name: "YouthCenterId",
                table: "Reservations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "YouthCenterActivityId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_YouthCenterActivity_YouthCenterActivityId",
                table: "Reservations",
                column: "YouthCenterActivityId",
                principalTable: "YouthCenterActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_YouthCenters_YouthCenterId",
                table: "Reservations",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId",
                table: "YouthCenterActivity",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_YouthCenterActivity_YouthCenterActivityId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_YouthCenters_YouthCenterId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId",
                table: "YouthCenterActivity");

            migrationBuilder.AlterColumn<int>(
                name: "YouthCenterId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "YouthCenterActivityId",
                table: "Reservations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ActivityId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ActivityId",
                table: "Reservations",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_YouthCenterActivity_ActivityId",
                table: "Reservations",
                column: "ActivityId",
                principalTable: "YouthCenterActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_YouthCenterActivity_YouthCenterActivityId",
                table: "Reservations",
                column: "YouthCenterActivityId",
                principalTable: "YouthCenterActivity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_YouthCenters_YouthCenterId",
                table: "Reservations",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId",
                table: "YouthCenterActivity",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

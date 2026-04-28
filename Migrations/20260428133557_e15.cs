using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class e15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_YouthCenterActivity_YouthCenterActivityId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId",
                table: "YouthCenterActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_YouthCenterId",
                table: "YouthCenterActivity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YouthCenterActivity",
                table: "YouthCenterActivity");

            migrationBuilder.RenameTable(
                name: "YouthCenterActivity",
                newName: "YouthCenterActivities");

            migrationBuilder.RenameIndex(
                name: "IX_YouthCenterActivity_YouthCenterId",
                table: "YouthCenterActivities",
                newName: "IX_YouthCenterActivities_YouthCenterId");

            migrationBuilder.RenameIndex(
                name: "IX_YouthCenterActivity_ActivityId",
                table: "YouthCenterActivities",
                newName: "IX_YouthCenterActivities_ActivityId");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_YouthCenterActivities",
                table: "YouthCenterActivities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_YouthCenterActivities_YouthCenterActivityId",
                table: "Reservations",
                column: "YouthCenterActivityId",
                principalTable: "YouthCenterActivities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivities_Activities_ActivityId",
                table: "YouthCenterActivities",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivities_YouthCenters_YouthCenterId",
                table: "YouthCenterActivities",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_YouthCenterActivities_YouthCenterActivityId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivities_Activities_ActivityId",
                table: "YouthCenterActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivities_YouthCenters_YouthCenterId",
                table: "YouthCenterActivities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YouthCenterActivities",
                table: "YouthCenterActivities");

            migrationBuilder.RenameTable(
                name: "YouthCenterActivities",
                newName: "YouthCenterActivity");

            migrationBuilder.RenameIndex(
                name: "IX_YouthCenterActivities_YouthCenterId",
                table: "YouthCenterActivity",
                newName: "IX_YouthCenterActivity_YouthCenterId");

            migrationBuilder.RenameIndex(
                name: "IX_YouthCenterActivities_ActivityId",
                table: "YouthCenterActivity",
                newName: "IX_YouthCenterActivity_ActivityId");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddPrimaryKey(
                name: "PK_YouthCenterActivity",
                table: "YouthCenterActivity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_YouthCenterActivity_YouthCenterActivityId",
                table: "Reservations",
                column: "YouthCenterActivityId",
                principalTable: "YouthCenterActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId",
                table: "YouthCenterActivity",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_YouthCenterId",
                table: "YouthCenterActivity",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

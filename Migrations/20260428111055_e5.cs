using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class e5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "YouthCenters",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "YouthCenters",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "YouthCenters",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "YouthCenters");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Activities");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "YouthCenters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ActivityId1",
                table: "YouthCenterActivity",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "YouthCenterActivity",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxCapacity",
                table: "YouthCenterActivity",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "YouthCenterActivity",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "YouthCenterActivityId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Activities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_YouthCenterActivity_ActivityId1",
                table: "YouthCenterActivity",
                column: "ActivityId1");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_YouthCenterActivityId",
                table: "Reservations",
                column: "YouthCenterActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_YouthCenterActivity_YouthCenterActivityId",
                table: "Reservations",
                column: "YouthCenterActivityId",
                principalTable: "YouthCenterActivity",
                principalColumn: "Id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_YouthCenterActivity_YouthCenterActivityId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_Activities_ActivityId1",
                table: "YouthCenterActivity");

            migrationBuilder.DropForeignKey(
                name: "FK_YouthCenterActivity_YouthCenters_ActivityId",
                table: "YouthCenterActivity");

            migrationBuilder.DropIndex(
                name: "IX_YouthCenterActivity_ActivityId1",
                table: "YouthCenterActivity");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_YouthCenterActivityId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "YouthCenters");

            migrationBuilder.DropColumn(
                name: "ActivityId1",
                table: "YouthCenterActivity");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "YouthCenterActivity");

            migrationBuilder.DropColumn(
                name: "MaxCapacity",
                table: "YouthCenterActivity");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "YouthCenterActivity");

            migrationBuilder.DropColumn(
                name: "YouthCenterActivityId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Activities");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerHour",
                table: "YouthCenters",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "User" },
                    { 2, "Admin" },
                    { 3, "SuperAdmin" }
                });

            migrationBuilder.InsertData(
                table: "YouthCenters",
                columns: new[] { "Id", "Description", "Location", "Mobile", "Name", "PricePerHour" },
                values: new object[,]
                {
                    { 1, null, null, "1", "c1", null },
                    { 2, null, null, "2", "c2", null },
                    { 3, null, null, "3", "c3", null }
                });

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
    }
}

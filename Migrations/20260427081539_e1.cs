using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class e1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Address",
                table: "YouthCenters",
                newName: "Location");

            migrationBuilder.AddColumn<string>(
                name: "ActivitiesIds",
                table: "YouthCenters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.UpdateData(
                table: "YouthCenters",
                keyColumn: "Id",
                keyValue: 1,
                column: "ActivitiesIds",
                value: "[]");

            migrationBuilder.UpdateData(
                table: "YouthCenters",
                keyColumn: "Id",
                keyValue: 2,
                column: "ActivitiesIds",
                value: "[]");

            migrationBuilder.UpdateData(
                table: "YouthCenters",
                keyColumn: "Id",
                keyValue: 3,
                column: "ActivitiesIds",
                value: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivitiesIds",
                table: "YouthCenters");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "YouthCenters",
                newName: "Address");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class e2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivitiesIds",
                table: "YouthCenters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivitiesIds",
                table: "YouthCenters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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
    }
}

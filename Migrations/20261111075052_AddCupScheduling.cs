using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddCupScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreakBetweenMatchesMinutes",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HalftimeMinutes",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.AddColumn<int>(
                name: "PlayDaysMask",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 127);

            migrationBuilder.AddColumn<int>(
                name: "RestDaysBetweenRounds",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakBetweenMatchesMinutes",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "HalftimeMinutes",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "PlayDaysMask",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "RestDaysBetweenRounds",
                table: "Cups");
        }
    }
}

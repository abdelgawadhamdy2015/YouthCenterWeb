using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class autoGenerateMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BreakMinutes",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DailyEndTime",
                table: "Cups",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DailyStartTime",
                table: "Cups",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MatchDurationMinutes",
                table: "Cups",
                type: "int",
                nullable: false,
                defaultValue: 45);

            migrationBuilder.AddColumn<string>(
                name: "ScheduleMode",
                table: "Cups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyScheduled",
                table: "CupMatches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BreakMinutes",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "DailyEndTime",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "DailyStartTime",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "MatchDurationMinutes",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "ScheduleMode",
                table: "Cups");

            migrationBuilder.DropColumn(
                name: "IsManuallyScheduled",
                table: "CupMatches");
        }
    }
}

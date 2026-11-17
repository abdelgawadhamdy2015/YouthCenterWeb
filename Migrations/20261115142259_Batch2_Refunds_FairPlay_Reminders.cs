using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class Batch2_Refunds_FairPlay_Reminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefundReference",
                table: "Reservations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefundedBy",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FairPlayPoints",
                table: "CupTeams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RedCardsA",
                table: "CupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RedCardsB",
                table: "CupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YellowCardsA",
                table: "CupMatches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YellowCardsB",
                table: "CupMatches",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundReference",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RefundedAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "RefundedBy",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "FairPlayPoints",
                table: "CupTeams");

            migrationBuilder.DropColumn(
                name: "RedCardsA",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "RedCardsB",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "YellowCardsA",
                table: "CupMatches");

            migrationBuilder.DropColumn(
                name: "YellowCardsB",
                table: "CupMatches");
        }
    }
}

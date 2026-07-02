using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class updatreservationworkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_YouthCenterActivityId",
                table: "Reservations");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentLockExpiresAt",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTransactionId",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
     name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
     table: "Reservations",
     columns: new[] { "YouthCenterActivityId", "Date", "StartTime", "EndTime" },
     unique: true,
     filter: "[Status] IN ('AwaitingPayment', 'Confirmed')"); // <-- Single quotes for SQL strings
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentLockExpiresAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "PaymentTransactionId",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_YouthCenterActivityId",
                table: "Reservations",
                column: "YouthCenterActivityId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class FixReservationStatusIndexFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
                table: "Reservations",
                columns: new[] { "YouthCenterActivityId", "Date", "StartTime", "EndTime" },
                unique: true,
                filter: "[Status] IN (N'AwaitingPayment', N'Confirmed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
                table: "Reservations");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
                table: "Reservations",
                columns: new[] { "YouthCenterActivityId", "Date", "StartTime", "EndTime" },
                unique: true,
                filter: "[Status] IN (1, 2)");
        }
    }
}

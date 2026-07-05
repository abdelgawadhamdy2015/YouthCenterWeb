using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReservationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
                table: "Reservations");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
                table: "Reservations",
                columns: new[] { "YouthCenterActivityId", "Date", "StartTime", "EndTime" },
                unique: true,
                filter: "[Status] IN ('AwaitingPayment', 'Confirmed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
                table: "Reservations");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_YouthCenterActivityId_Date_StartTime_EndTime",
                table: "Reservations",
                columns: new[] { "YouthCenterActivityId", "Date", "StartTime", "EndTime" },
                unique: true,
                filter: "[Status] IN (N'AwaitingPayment', N'Confirmed')");
        }
    }
}

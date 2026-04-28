using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class e6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Activities_ActivityId",
                table: "Reservations");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_YouthCenterActivity_ActivityId",
                table: "Reservations",
                column: "ActivityId",
                principalTable: "YouthCenterActivity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_YouthCenterActivity_ActivityId",
                table: "Reservations");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Activities_ActivityId",
                table: "Reservations",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

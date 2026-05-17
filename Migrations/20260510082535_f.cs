using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class f : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Activities_activityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FavActivityId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "activityId",
                table: "Users",
                newName: "ActivityId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_activityId",
                table: "Users",
                newName: "IX_Users_ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Activities_ActivityId",
                table: "Users",
                column: "ActivityId",
                principalTable: "Activities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Activities_ActivityId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "ActivityId",
                table: "Users",
                newName: "activityId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_ActivityId",
                table: "Users",
                newName: "IX_Users_activityId");

            migrationBuilder.AddColumn<int>(
                name: "FavActivityId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Activities_activityId",
                table: "Users",
                column: "activityId",
                principalTable: "Activities",
                principalColumn: "Id");
        }
    }
}

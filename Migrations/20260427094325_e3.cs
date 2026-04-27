using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class e3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_YouthCenters_YouthCenterId",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_YouthCenterId",
                table: "Activities");

            migrationBuilder.CreateTable(
                name: "YouthCenterActivity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YouthCenterId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YouthCenterActivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YouthCenterActivity_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YouthCenterActivity_YouthCenters_YouthCenterId",
                        column: x => x.YouthCenterId,
                        principalTable: "YouthCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YouthCenterActivity_ActivityId",
                table: "YouthCenterActivity",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_YouthCenterActivity_YouthCenterId",
                table: "YouthCenterActivity",
                column: "YouthCenterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YouthCenterActivity");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_YouthCenterId",
                table: "Activities",
                column: "YouthCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_YouthCenters_YouthCenterId",
                table: "Activities",
                column: "YouthCenterId",
                principalTable: "YouthCenters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

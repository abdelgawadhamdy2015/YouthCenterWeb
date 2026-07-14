using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YouthCenterWeb.Migrations
{
    /// <inheritdoc />
    public partial class updateOtpEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mobile",
                table: "OtpEntries",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "OtpEntries",
                newName: "CodeHash");

            migrationBuilder.AddColumn<int>(
                name: "Attempts",
                table: "OtpEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attempts",
                table: "OtpEntries");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "OtpEntries",
                newName: "Mobile");

            migrationBuilder.RenameColumn(
                name: "CodeHash",
                table: "OtpEntries",
                newName: "Code");
        }
    }
}
